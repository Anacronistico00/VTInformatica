using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.DTOs.Account;
using VTInformatica.DTOs.Cart;
using VTInformatica.DTOs.Order;
using VTInformatica.Models;
using VTInformatica.Models.Auth;

namespace VTInformatica.Tests.Infrastructure;

public static class TestHelpers
{
    // Rispetta la password policy di appsettings.json (8+ caratteri, maiuscola, minuscola, cifra, simbolo).
    public const string ValidPassword = "Password1!";

    public static string UniqueEmail() => $"user-{Guid.NewGuid():N}@test.local";

    public static Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email, string password = ValidPassword) =>
        client.PostAsJsonAsync("/Account/register", new
        {
            email,
            password,
            firstName = "Mario",
            lastName = "Rossi",
            birthDate = "1990-01-01"
        });

    public static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password = ValidPassword) =>
        client.PostAsJsonAsync("/Account/login", new { email, password });

    /// <summary>Registra un utente, fa login e restituisce il JWT emesso dall'API.</summary>
    public static async Task<string> RegisterAndLoginAsync(VtWebApplicationFactory factory, string email)
    {
        var client = factory.CreateClient();

        var register = await RegisterAsync(client, email);
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        var login = await LoginAsync(client, email);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var token = await login.Content.ReadFromJsonAsync<TokenResponseDto>();
        return token!.Token;
    }

    /// <summary>
    /// Registra un utente, lo promuove ad Admin via UserManager e solo dopo fa login
    /// (i ruoli entrano nel JWT al momento del login). Restituisce il token.
    /// </summary>
    public static async Task<string> RegisterAndLoginAsAdminAsync(VtWebApplicationFactory factory, string email)
    {
        var client = factory.CreateClient();

        var register = await RegisterAsync(client, email);
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            var result = await userManager.AddToRoleAsync(user!, "Admin");
            Assert.True(result.Succeeded);
        }

        var login = await LoginAsync(client, email);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var token = await login.Content.ReadFromJsonAsync<TokenResponseDto>();
        return token!.Token;
    }

    public static HttpClient AuthorizedClient(VtWebApplicationFactory factory, string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Inserisce un prodotto con stock e prezzo noti (categoria/sottocategoria/produttore da HasData).</summary>
    public static async Task<int> SeedProductAsync(VtWebApplicationFactory factory, int quantity, decimal price = 100m)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var product = new Product
        {
            Name = $"Prodotto test {Guid.NewGuid():N}",
            Description = "Prodotto creato dai test",
            Price = price,
            Quantity = quantity,
            CategoryId = 1,
            SubCategoryId = 1,
            ManufacturerId = 1
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product.Id;
    }

    public static async Task<int> GetProductStockAsync(VtWebApplicationFactory factory, int productId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = await db.Products.FindAsync(productId);
        return product!.Quantity;
    }

    public static Task<HttpResponseMessage> AddToCartAsync(HttpClient client, string email, int productId, int quantity) =>
        client.PostAsJsonAsync($"/Cart/{email}/add", new CartItemDto { ProductId = productId, Quantity = quantity });

    /// <summary>Crea un utente con un ordine di <paramref name="quantity"/> pezzi del prodotto indicato.</summary>
    public static async Task<(string Email, string Token, GetOrderDto Order)> CreateUserWithOrderAsync(
        VtWebApplicationFactory factory, int productId, int quantity)
    {
        var email = UniqueEmail();
        var token = await RegisterAndLoginAsync(factory, email);
        var client = AuthorizedClient(factory, token);

        var add = await AddToCartAsync(client, email, productId, quantity);
        Assert.Equal(HttpStatusCode.OK, add.StatusCode);

        var create = await client.PostAsync($"/Order/from-cart/{email}", null);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var order = await create.Content.ReadFromJsonAsync<GetOrderDto>();
        return (email, token, order!);
    }
}
