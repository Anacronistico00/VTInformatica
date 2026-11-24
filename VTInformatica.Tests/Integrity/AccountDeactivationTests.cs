using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VTInformatica.Models.Auth;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Integrity;

/// <summary>
/// Disattivazione, ban e riattivazione (docs/AUDIT-prima.md, feat/account-reactivation).
/// Lo stato dell'account si rivela solo dopo la verifica della password: con password
/// errata la risposta è identica per email inesistente, account attivo, disattivato o sospeso.
/// </summary>
[Collection(ApiCollection.Name)]
public class AccountDeactivationTests
{
    private const string WrongPassword = "Sbagliata1!";

    private readonly VtWebApplicationFactory _factory;

    public AccountDeactivationTests(VtWebApplicationFactory factory) => _factory = factory;

    // ---- Login ----

    [Theory]
    [InlineData("active")]
    [InlineData("deactivated")]
    [InlineData("suspended")]
    public async Task Login_WrongPassword_IdenticalForMissingActiveDeactivatedSuspended(string state)
    {
        var client = _factory.CreateClient();
        var missing = await TestHelpers.LoginAsync(client, TestHelpers.UniqueEmail(), WrongPassword);

        var email = await CreateUserAsync(state);
        var response = await TestHelpers.LoginAsync(client, email, WrongPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);
        Assert.Equal(missing.StatusCode, response.StatusCode);
        Assert.Equal(await NormalizedBodyAsync(missing), await NormalizedBodyAsync(response));
        Assert.Equal("invalid_credentials", await CodeAsync(response));
    }

    [Fact]
    public async Task Login_CorrectPassword_SelfDeactivated_Returns403AccountDeactivated()
    {
        var email = await CreateUserAsync("deactivated");

        var response = await TestHelpers.LoginAsync(_factory.CreateClient(), email);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("account_deactivated", await CodeAsync(response));
    }

    [Fact]
    public async Task Login_CorrectPassword_Suspended_Returns403AccountSuspended_WithoutNote()
    {
        const string note = "Nota riservata per lo staff 7f3a";
        var email = await CreateUserAsync("suspended", note);

        var response = await TestHelpers.LoginAsync(_factory.CreateClient(), email);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("account_suspended", await CodeAsync(response));
        Assert.DoesNotContain(note, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_AfterMaxFailedAttempts_CorrectPasswordReturns401()
    {
        var email = await CreateUserAsync("active");
        var client = _factory.CreateClient();

        for (var i = 0; i < MaxFailedAccessAttempts(); i++)
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await TestHelpers.LoginAsync(client, email, WrongPassword)).StatusCode);
        }

        var response = await TestHelpers.LoginAsync(client, email);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("invalid_credentials", await CodeAsync(response));
    }

    // ---- Helper ----

    /// <summary>
    /// Registra un utente e ne imposta lo stato direttamente nel DB:
    /// "active", "deactivated" (Reason=User) o "suspended" (Reason=Admin).
    /// </summary>
    private async Task<string> CreateUserAsync(string state, string? note = null)
    {
        var email = TestHelpers.UniqueEmail();
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.RegisterAsync(_factory.CreateClient(), email)).StatusCode);
        if (state == "active") return email;

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await userManager.FindByEmailAsync(email))!;
        user.IsActive = false;
        user.DeactivationReason = state == "deactivated" ? DeactivationReason.User : DeactivationReason.Admin;
        user.DeactivationNote = note;
        user.DeactivatedAt = DateTime.UtcNow;
        Assert.True((await userManager.UpdateAsync(user)).Succeeded);
        return email;
    }

    private int MaxFailedAccessAttempts() =>
        _factory.Services.GetRequiredService<IOptions<IdentityOptions>>().Value.Lockout.MaxFailedAccessAttempts;

    /// <summary>Corpo confrontabile tra due risposte: JSON senza traceId (diverso per ogni richiesta), altrimenti testo.</summary>
    private static async Task<string> NormalizedBodyAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        try
        {
            if (JsonNode.Parse(text) is JsonObject json)
            {
                json.Remove("traceId");
                return json.ToJsonString();
            }
        }
        catch (JsonException)
        {
        }
        return text;
    }

    private static async Task<string?> CodeAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.ValueKind == JsonValueKind.Object && json.TryGetProperty("code", out var code) ? code.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
