using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// AUDIT #7 (stock) e #8 (errore chiaro): il checkout non deve mai vendere più dello
/// stock disponibile, nemmeno se lo stock cambia tra lettura e scrittura, e lo stock
/// insufficiente è un 409, non un 500.
/// SQLite non riproduce un race reale: l'ordine concorrente è simulato in modo
/// deterministico da ConcurrentStockWriterInterceptor.
/// </summary>
public class OrderStockTests : IClassFixture<FaultInjectionFactory>
{
    private readonly FaultInjectionFactory _factory;

    public OrderStockTests(FaultInjectionFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateFromCart_StockReducedConcurrently_Returns409_NoOversell()
    {
        // Stock 5, carrello 3. Tra la lettura e la scrittura un altro ordine porta lo stock a 1.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 5);
        var (email, client) = await NewUserWithCartAsync((productId, 3));

        HttpResponseMessage response;
        _factory.ConcurrentStockWriter.Arm(productId, stockWrittenByOtherOrder: 1);
        try
        {
            response = await client.PostAsync($"/Order/from-cart/{email}", null);
        }
        finally
        {
            _factory.ConcurrentStockWriter.Disarm();
        }

        // Stock misurato subito dopo il nostro UPDATE, prima di commit/rollback: la scrittura
        // simulata dell'altro ordine sta nella nostra stessa transazione (una sola connessione
        // SQLite) e un rollback la annullerebbe. Deve restare 1: nessuna sovrascrittura.
        // Status e stock insieme: se fallisce, l'output mostra entrambi.
        Assert.Equal(
            (HttpStatusCode.Conflict, (int?)1),
            (response.StatusCode, _factory.ConcurrentStockWriter.StockAfterOurUpdate));
        await AssertNoOrderAndCartIntactAsync(email, (productId, 3));
    }

    [Fact]
    public async Task CreateFromCart_InsufficientStock_Returns409_StateUnchanged()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 5);
        var (email, client) = await NewUserWithCartAsync((productId, 3));
        await SetProductStockAsync(productId, 2);

        var response = await client.PostAsync($"/Order/from-cart/{email}", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(2, await TestHelpers.GetProductStockAsync(_factory, productId));
        await AssertNoOrderAndCartIntactAsync(email, (productId, 3));
    }

    [Fact]
    public async Task CreateFromCart_SecondProductInsufficient_NothingDecremented()
    {
        var productA = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var productB = await TestHelpers.SeedProductAsync(_factory, quantity: 5);
        var (email, client) = await NewUserWithCartAsync((productA, 2), (productB, 3));
        await SetProductStockAsync(productB, 1);

        var response = await client.PostAsync($"/Order/from-cart/{email}", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(10, await TestHelpers.GetProductStockAsync(_factory, productA));
        Assert.Equal(1, await TestHelpers.GetProductStockAsync(_factory, productB));
        await AssertNoOrderAndCartIntactAsync(email, (productA, 2), (productB, 3));
    }

    private async Task<(string Email, HttpClient Client)> NewUserWithCartAsync(params (int ProductId, int Quantity)[] items)
    {
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        foreach (var (productId, quantity) in items)
        {
            Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, email, productId, quantity)).StatusCode);
        }
        return (email, client);
    }

    private async Task SetProductStockAsync(int productId, int stock)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = await db.Products.FindAsync(productId);
        product!.Quantity = stock;
        await db.SaveChangesAsync();
    }

    private async Task AssertNoOrderAndCartIntactAsync(string email, params (int ProductId, int Quantity)[] expectedItems)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(await db.Orders.AnyAsync(o => o.CustomerEmail == email), "Nessun ordine deve restare salvato.");
        var cart = await db.Carts.AsNoTracking().Include(c => c.Items).SingleAsync(c => c.Email == email);
        Assert.Equal(
            expectedItems.OrderBy(i => i.ProductId).ToList(),
            cart.Items.Select(i => (i.ProductId, i.Quantity)).OrderBy(i => i.ProductId).ToList());
    }
}
