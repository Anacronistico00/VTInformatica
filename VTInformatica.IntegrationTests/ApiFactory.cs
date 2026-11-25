using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.DTOs.Account;
using VTInformatica.DTOs.Cart;
using VTInformatica.Models;
using VTInformatica.Models.Auth;

namespace VTInformatica.IntegrationTests;

/// <summary>L'API vera in memoria (TestServer) sopra un database SQL Server già migrato.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    // Chiave solo per i test, mai quella di produzione.
    private const string JwtKey = "integration-only-signing-key-0123456789-abcdefghijklmnopqrstuvwxyz";
    public const string Password = "Password1!";

    private readonly IInterceptor[] _interceptors;

    private ApiFactory(string connectionString, IInterceptor[] interceptors)
    {
        ConnectionString = connectionString;
        _interceptors = interceptors;
    }

    public string ConnectionString { get; }

    public static async Task<ApiFactory> CreateAsync(SqlServerFixture sql, params IInterceptor[] interceptors) =>
        new(await sql.CreateMigratedDatabaseAsync(), interceptors);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        builder.UseSetting("Jwt:SecurityKey", JwtKey);
        builder.UseSetting("Jwt:Issuer", "https://integration.vtinformatica.local");
        builder.UseSetting("Jwt:Audience", "https://integration-audience.vtinformatica.local");
        builder.UseSetting("Jwt:ExpiresInMinutes", "60");

        builder.ConfigureServices(services =>
        {
            if (_interceptors.Length > 0)
                services.ConfigureDbContext<ApplicationDbContext>(options => options.AddInterceptors(_interceptors));
        });
    }

    public static string UniqueEmail() => $"user-{Guid.NewGuid():N}@integration.local";

    public async Task<string> RegisterAndLoginAsync(string email, bool admin = false)
    {
        var client = CreateClient();
        var register = await client.PostAsJsonAsync("/Account/register",
            new { email, password = Password, firstName = "Mario", lastName = "Rossi", birthDate = "1990-01-01" });
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        if (admin)
        {
            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            Assert.True((await userManager.AddToRoleAsync(user!, "Admin")).Succeeded);
        }

        var login = await client.PostAsJsonAsync("/Account/login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        return (await login.Content.ReadFromJsonAsync<TokenResponseDto>())!.Token;
    }

    public HttpClient ClientFor(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Prodotto con stock noto (sottocategoria e produttore dal seed delle migration).</summary>
    public async Task<int> SeedProductAsync(int quantity)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = new Product
        {
            Name = $"Prodotto integrazione {Guid.NewGuid():N}",
            Description = "Creato dai test di integrazione",
            Price = 10m,
            Quantity = quantity,
            SubCategoryId = 1,
            ManufacturerId = 1
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product.Id;
    }

    public static Task<HttpResponseMessage> AddToCartAsync(HttpClient client, int productId, int quantity) =>
        client.PostAsJsonAsync("/Cart/me/add", new CartItemDto { ProductId = productId, Quantity = quantity });
}
