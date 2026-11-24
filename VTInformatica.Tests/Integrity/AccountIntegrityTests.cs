using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.DTOs.Cart;
using VTInformatica.Models.Auth;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Integrity;

/// <summary>
/// Integrità degli account: registrazione su email disattivata (AUDIT #9) e modifica
/// del profilo senza rompere il legame email → carrello/ordini.
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

    [Fact]
    public async Task UpdateUser_ChangingEmail_Returns400_AndCartStillReachable()
    {
        // Carrello e ordini usano l'email come chiave: cambiarla da qui romperebbe il legame
        // (e UserName resterebbe disallineato). Il cambio email richiede un flusso dedicato.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, email, productId, quantity: 2)).StatusCode);
        var userId = await TestHelpers.GetUserIdAsync(_factory, email);

        var response = await client.PutAsJsonAsync($"/Account/UpdateUser/{userId}", new
        {
            firstName = "Mario",
            lastName = "Rossi",
            email = TestHelpers.UniqueEmail(),
            birthDate = "1990-01-01"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var user = await FindUserAsync(email);
        Assert.Equal(email, user.Email);
        Assert.Equal(email, user.UserName);

        var cart = await client.GetFromJsonAsync<GetCartDto>($"/Cart/{email}");
        Assert.Equal(2, Assert.Single(cart!.Items).Quantity);
    }

    [Fact]
    public async Task UpdateUser_SameEmailOrNoEmail_UpdatesProfile()
    {
        // Guardia: nome e data di nascita restano modificabili; l'email uguale (anche con
        // maiuscole diverse) o assente non è un cambio.
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        var userId = await TestHelpers.GetUserIdAsync(_factory, email);

        var sameEmail = await client.PutAsJsonAsync($"/Account/UpdateUser/{userId}", new
        {
            firstName = "Luigi",
            lastName = "Verdi",
            email = email.ToUpperInvariant(),
            birthDate = "1985-05-05"
        });
        Assert.Equal(HttpStatusCode.OK, sameEmail.StatusCode);

        var noEmail = await client.PutAsJsonAsync($"/Account/UpdateUser/{userId}", new
        {
            firstName = "Anna",
            lastName = "Bianchi",
            birthDate = "1985-05-05"
        });
        Assert.Equal(HttpStatusCode.OK, noEmail.StatusCode);

        var user = await FindUserAsync(email);
        Assert.Equal("Anna", user.FirstName);
        Assert.Equal("Bianchi", user.LastName);
        Assert.Equal(new DateOnly(1985, 5, 5), user.BirthDate);
        Assert.Equal(email, user.Email);
    }

    [Fact]
    public async Task UpdateUser_MissingName_Returns400_AndKeepsProfile()
    {
        // Guardia: con <Nullable>enable</Nullable> le stringhe non-nullable del DTO sono già
        // obbligatorie per [ApiController]. Deve restare vero quando Email diventa opzionale.
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        var userId = await TestHelpers.GetUserIdAsync(_factory, email);

        var response = await client.PutAsJsonAsync($"/Account/UpdateUser/{userId}", new
        {
            lastName = "Rossi",
            birthDate = "1990-01-01"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Mario", (await FindUserAsync(email)).FirstName);
    }

    [Fact]
    public async Task DeactivatedUser_OldToken_Returns401()
    {
        // Il JWT di un account disattivato non deve più autenticare, anche se non è scaduto.
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/Cart/{email}")).StatusCode);

        await DeactivateAsync(email);

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/Cart/{email}")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/Order/user/{email}")).StatusCode);
    }

    [Fact]
    public async Task DeletedUser_OldToken_Returns401()
    {
        // Un token il cui utente non esiste più non deve autenticare nessuno.
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True((await userManager.DeleteAsync((await userManager.FindByEmailAsync(email))!)).Succeeded);
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/Cart/{email}")).StatusCode);
    }

    private async Task DeactivateAsync(string email)
    {
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));
        // Un Admin disattiva un utente con la sospensione.
        var userId = await TestHelpers.GetUserIdAsync(_factory, email);
        Assert.Equal(HttpStatusCode.NoContent,
            (await admin.PutAsJsonAsync($"/Account/{userId}/suspend", new { note = "Disattivato dai test" })).StatusCode);
    }

    private async Task<ApplicationUser> FindUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return (await userManager.FindByEmailAsync(email))!;
    }
}
