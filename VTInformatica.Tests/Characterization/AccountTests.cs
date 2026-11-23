using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.DTOs.Account;
using VTInformatica.Models.Auth;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Characterization;

/// <summary>
/// Test di caratterizzazione: descrivono il comportamento ATTUALE di registrazione e login,
/// compresi i bug noti (vedi docs/AUDIT-prima.md). Un fix deve far cambiare questi test
/// in modo esplicito.
/// </summary>
[Collection(ApiCollection.Name)]
[Trait("Category", "Characterization")]
public class AccountTests
{
    private readonly VtWebApplicationFactory _factory;

    public AccountTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_NewUser_Returns200()
    {
        var client = _factory.CreateClient();

        var response = await TestHelpers.RegisterAsync(client, TestHelpers.UniqueEmail());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Registrazione effettuata con successo", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Register_NewUser_IsAssignedCustomerRole()
    {
        // AUDIT #1 risolto in M1a: il ruolo di default alla registrazione è Customer (prima era Admin).
        // L'uguaglianza sull'array completo garantisce anche che Admin NON sia assegnato.
        var client = _factory.CreateClient();
        var email = TestHelpers.UniqueEmail();

        var response = await TestHelpers.RegisterAsync(client, email);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);

        var roles = await userManager.GetRolesAsync(user!);
        Assert.Equal(new[] { "Customer" }, roles);
    }

    [Fact]
    public async Task Register_DuplicateActiveEmail_Returns400()
    {
        var client = _factory.CreateClient();
        var email = TestHelpers.UniqueEmail();
        await TestHelpers.RegisterAsync(client, email);

        var response = await TestHelpers.RegisterAsync(client, email);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Email già registrata", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwt_WithEmailAndCustomerRoleClaims()
    {
        // AUDIT #1 risolto in M1a: il token di un utente appena registrato contiene solo role=Customer.
        var client = _factory.CreateClient();
        var email = TestHelpers.UniqueEmail();
        await TestHelpers.RegisterAsync(client, email);

        var response = await TestHelpers.LoginAsync(client, email);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(body!.Token);

        // AccountController.Login costruisce il token con new JwtSecurityToken(...): i claim
        // mantengono i tipi URI completi di ClaimTypes (nessuna mappatura a "email"/"role").
        Assert.Equal(email, jwt.Claims.Single(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(new[] { "Customer" }, jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));

        // Il token è firmato con la configurazione di test, non con quella di appsettings.json.
        Assert.Equal(VtWebApplicationFactory.JwtIssuer, jwt.Issuer);
        Assert.Contains(VtWebApplicationFactory.JwtAudience, jwt.Audiences);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = _factory.CreateClient();
        var email = TestHelpers.UniqueEmail();
        await TestHelpers.RegisterAsync(client, email);

        var response = await TestHelpers.LoginAsync(client, email, "WrongPassword1!");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
