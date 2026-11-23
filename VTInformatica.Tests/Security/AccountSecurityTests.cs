using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Models.Auth;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Security;

/// <summary>
/// Identità utente nel JWT e protezione di PUT /Account/UpdateUser/{id}.
/// </summary>
[Collection(ApiCollection.Name)]
public class AccountSecurityTests
{
    private readonly VtWebApplicationFactory _factory;

    public AccountSecurityTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_Token_ContainsNameIdentifierClaimWithUserId()
    {
        var email = TestHelpers.UniqueEmail();
        var token = await TestHelpers.RegisterAndLoginAsync(_factory, email);
        var userId = await TestHelpers.GetUserIdAsync(_factory, email);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(userId, jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }

    // ---- PUT /Account/UpdateUser/{id} ----

    [Fact]
    public async Task UpdateUser_WithoutToken_Returns401()
    {
        var emailA = TestHelpers.UniqueEmail();
        await TestHelpers.RegisterAndLoginAsync(_factory, emailA);
        var idA = await TestHelpers.GetUserIdAsync(_factory, emailA);

        var response = await UpdateUserAsync(_factory.CreateClient(), idA, emailA, "Hacked");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Mario", await GetFirstNameAsync(emailA));
    }

    [Fact]
    public async Task UpdateUser_OtherUser_Returns403()
    {
        var emailA = TestHelpers.UniqueEmail();
        await TestHelpers.RegisterAndLoginAsync(_factory, emailA);
        var idA = await TestHelpers.GetUserIdAsync(_factory, emailA);
        var clientB = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail()));

        var response = await UpdateUserAsync(clientB, idA, emailA, "Hacked");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("Mario", await GetFirstNameAsync(emailA));
    }

    [Fact]
    public async Task UpdateUser_SameUser_Returns200()
    {
        var emailA = TestHelpers.UniqueEmail();
        var clientA = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, emailA));
        var idA = await TestHelpers.GetUserIdAsync(_factory, emailA);

        var response = await UpdateUserAsync(clientA, idA, emailA, "Luigi");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Luigi", await GetFirstNameAsync(emailA));
    }

    [Fact]
    public async Task UpdateUser_AsAdmin_OtherUser_Returns200()
    {
        var emailA = TestHelpers.UniqueEmail();
        await TestHelpers.RegisterAndLoginAsync(_factory, emailA);
        var idA = await TestHelpers.GetUserIdAsync(_factory, emailA);
        var adminClient = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

        var response = await UpdateUserAsync(adminClient, idA, emailA, "Anna");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Anna", await GetFirstNameAsync(emailA));
    }

    // Cambia solo il nome; l'email resta la stessa per non avere effetti collaterali.
    private static Task<HttpResponseMessage> UpdateUserAsync(HttpClient client, string id, string email, string firstName) =>
        client.PutAsJsonAsync($"/Account/UpdateUser/{id}", new
        {
            firstName,
            lastName = "Rossi",
            email,
            birthDate = "1990-01-01"
        });

    private async Task<string> GetFirstNameAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        return user!.FirstName;
    }
}
