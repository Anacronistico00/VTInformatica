using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.DTOs.Cart;
using VTInformatica.Interfaces;
using VTInformatica.Models;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// Leggere il carrello non scrive nulla (nemmeno nel change tracker) e il totale
/// non è una colonna: si calcola sempre dalle righe e dai prezzi correnti.
/// </summary>
[Collection(ApiCollection.Name)]
public class CartReadTests
{
    private readonly VtWebApplicationFactory _factory;

    public CartReadTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetCart_UserWithoutCart_ReturnsEmptyCart_AndTracksNothing()
    {
        var email = TestHelpers.UniqueEmail();
        await TestHelpers.RegisterAndLoginAsync(_factory, email);
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICartService>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cart = await service.GetCartByEmailAsync(email);

        Assert.NotNull(cart);
        Assert.Equal(email, cart!.Email);
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.TotalPrice);
        Assert.Empty(db.ChangeTracker.Entries<Cart>());
    }

    [Fact]
    public async Task GetCart_Twice_NeverCreatesACartRow()
    {
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        await client.GetAsync("/Cart/me");
        await client.GetAsync("/Cart/me");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(db.Carts.Where(c => c.Email == email));
    }

    [Fact]
    public void Cart_HasNoPersistedTotal()
    {
        Assert.Null(typeof(Cart).GetProperty("TotalPrice"));
    }

    [Fact]
    public async Task GetCart_TotalFollowsTheCurrentPrices()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10, price: 10m);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, email, productId, quantity: 3)).StatusCode);

        var before = await client.GetFromJsonAsync<GetCartDto>("/Cart/me");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await db.Products.FindAsync(productId);
            product!.Price = 20m;
            await db.SaveChangesAsync();
        }
        var after = await client.GetFromJsonAsync<GetCartDto>("/Cart/me");

        Assert.Equal((30m, 60m), (before!.TotalPrice, after!.TotalPrice));
    }
}
