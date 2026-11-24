using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Models.Auth;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Integrity;

/// <summary>
/// Integrità degli account: registrazione su email disattivata (AUDIT #9).
/// </summary>
[Collection(ApiCollection.Name)]
public class AccountIntegrityTests
{
    private readonly VtWebApplicationFactory _factory;

    public AccountIntegrityTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_DeactivatedEmail_Returns400_AndKeepsProfile()
    {
        // AUDIT #9: chiunque, senza password, poteva sovrascrivere il profilo di un utente disattivato.
        var email = TestHelpers.UniqueEmail();
        var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.RegisterAsync(client, email)).StatusCode);
        await DeactivateAsync(email);

        var response = await client.PostAsJsonAsync("/Account/register", new
        {
            email,
            password = "Altra-Password1!",
            firstName = "Intruso",
            lastName = "Sconosciuto",
            birthDate = "2000-12-31"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Email già registrata", await response.Content.ReadAsStringAsync());

        var user = await FindUserAsync(email);
        Assert.Equal("Mario", user.FirstName);
        Assert.Equal("Rossi", user.LastName);
        Assert.Equal(new DateOnly(1990, 1, 1), user.BirthDate);
        Assert.False(user.IsActive);
    }

    [Fact]
    public async Task Register_IgnoresClientIsActive()
    {
        // AUDIT #9: IsActive arrivava dal client; un nuovo utente è sempre attivo.
        var email = TestHelpers.UniqueEmail();
        var client = _factory.CreateClient();

        var register = await client.PostAsJsonAsync("/Account/register", new
        {
            email,
            password = TestHelpers.ValidPassword,
            firstName = "Mario",
            lastName = "Rossi",
            birthDate = "1990-01-01",
            isActive = false
        });
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        Assert.True((await FindUserAsync(email)).IsActive);
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.LoginAsync(client, email)).StatusCode);
    }

    private async Task DeactivateAsync(string email)
    {
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));
        Assert.Equal(HttpStatusCode.OK, (await admin.PutAsync($"/Account/DeleteUserByEmail/{email}", null)).StatusCode);
    }

    private async Task<ApplicationUser> FindUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return (await userManager.FindByEmailAsync(email))!;
    }
}
