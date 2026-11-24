using System.Net;
using System.Net.Http.Json;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>
/// AccountController: le risposte 4xx costruite a mano ({ message }) passano al formato unico
/// ProblemDetails + code (AUDIT #11). Gli status non cambiano; i testi restano nel "detail".
/// </summary>
[Collection(ApiCollection.Name)]
public class AccountErrorTests
{
    private readonly VtWebApplicationFactory _factory;

    public AccountErrorTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_DuplicateEmail_Returns400EmailAlreadyRegistered()
    {
        var client = _factory.CreateClient();
        var email = TestHelpers.UniqueEmail();
        await TestHelpers.RegisterAsync(client, email);

        var response = await TestHelpers.RegisterAsync(client, email);

        var json = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "email_already_registered");
        Assert.Contains("Email già registrata", json.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400WeakPassword()
    {
        var response = await TestHelpers.RegisterAsync(_factory.CreateClient(), TestHelpers.UniqueEmail(), password: "debole");

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "weak_password");
    }

    [Fact]
    public async Task Register_IdentityRejectsUser_Returns400RegistrationFailed_WithErrors()
    {
        // '!' è ammesso nell'email ma non nello UserName di Identity: CreateAsync fallisce.
        var response = await TestHelpers.RegisterAsync(_factory.CreateClient(), $"utente!{Guid.NewGuid():N}@test.local");

        var json = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "registration_failed");
        Assert.True(json.GetProperty("errors").GetArrayLength() > 0);
    }

    [Fact]
    public async Task RegisterOrLogin_WhenAlreadyAuthenticated_Returns400AlreadyAuthenticated()
    {
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        await ProblemAssert.IsProblemAsync(await TestHelpers.RegisterAsync(client, TestHelpers.UniqueEmail()), HttpStatusCode.BadRequest, "already_authenticated");
        await ProblemAssert.IsProblemAsync(await TestHelpers.LoginAsync(client, email), HttpStatusCode.BadRequest, "already_authenticated");
    }

    [Fact]
    public async Task UpdateUser_ChangingEmail_Returns400EmailChangeNotAllowed()
    {
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        var userId = await TestHelpers.GetUserIdAsync(_factory, email);

        var response = await client.PutAsJsonAsync($"/Account/UpdateUser/{userId}",
            new { firstName = "Mario", lastName = "Rossi", email = TestHelpers.UniqueEmail(), birthDate = "1990-01-01" });

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "email_change_not_allowed");
    }

    [Fact]
    public async Task UpdateUser_UnknownUser_Returns404NotFound()
    {
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

        var response = await admin.PutAsJsonAsync($"/Account/UpdateUser/{Guid.NewGuid()}",
            new { firstName = "Mario", lastName = "Rossi", birthDate = "1990-01-01" });

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "not_found");
    }
}
