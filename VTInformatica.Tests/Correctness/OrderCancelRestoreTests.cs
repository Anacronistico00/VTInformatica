using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// Annulla e ripristina ordine con stock atomico
/// (UPDATE ... SET Quantity = Quantity +/- n invece di read-modify-write) e 409 se
/// il ripristino non ha stock sufficiente. Scrittura concorrente simulata dall'interceptor.
/// </summary>
public class OrderCancelRestoreTests : IClassFixture<FaultInjectionFactory>
{
    private readonly FaultInjectionFactory _factory;

    public OrderCancelRestoreTests(FaultInjectionFactory factory) => _factory = factory;

    [Fact]
    public async Task Cancel_StockChangedConcurrently_AddsBackOnTopOfTheNewStock()
    {
        // Stock 10, ordine da 3 → 7. Durante l'annullamento un altro ordine porta lo stock a 20:
        // l'incremento atomico dà 23 (la scrittura del valore letto darebbe 10).
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, token, order) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 3);
        var client = TestHelpers.AuthorizedClient(_factory, token);

        HttpResponseMessage response;
        _factory.ConcurrentStockWriter.Arm(productId, stockWrittenByOtherOrder: 20);
        try
        {
            response = await client.PutAsync($"/Order/{order.Id}/delete", null);
        }
        finally
        {
            _factory.ConcurrentStockWriter.Disarm();
        }

        Assert.Equal(
            (HttpStatusCode.OK, (int?)23),
            (response.StatusCode, _factory.ConcurrentStockWriter.StockAfterOurUpdate));
    }

    [Fact]
    public async Task Cancel_Twice_ReturnsStockOnlyOnce()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, token, order) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 3);
        var client = TestHelpers.AuthorizedClient(_factory, token);

        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync($"/Order/{order.Id}/delete", null)).StatusCode);
        var second = await client.PutAsync($"/Order/{order.Id}/delete", null);

        await ProblemAssert.IsProblemAsync(second, HttpStatusCode.NotFound, "not_found");
        Assert.Equal(10, await TestHelpers.GetProductStockAsync(_factory, productId));
    }

    [Fact]
    public async Task Restore_StockChangedConcurrently_SubtractsFromTheNewStock()
    {
        // Ordine da 3 annullato (stock 10). Durante il ripristino lo stock diventa 20:
        // il decremento atomico dà 17 (la scrittura del valore letto darebbe 7).
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, token, order) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 3);
        var client = TestHelpers.AuthorizedClient(_factory, token);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync($"/Order/{order.Id}/delete", null)).StatusCode);

        HttpResponseMessage response;
        _factory.ConcurrentStockWriter.Arm(productId, stockWrittenByOtherOrder: 20);
        try
        {
            response = await client.PutAsync($"/Order/{order.Id}/restore", null);
        }
        finally
        {
            _factory.ConcurrentStockWriter.Disarm();
        }

        Assert.Equal(
            (HttpStatusCode.OK, (int?)17),
            (response.StatusCode, _factory.ConcurrentStockWriter.StockAfterOurUpdate));
    }

    [Fact]
    public async Task Restore_InsufficientStock_Returns409_OrderStaysCancelled()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, token, order) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 3);
        var client = TestHelpers.AuthorizedClient(_factory, token);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync($"/Order/{order.Id}/delete", null)).StatusCode);
        await SetStockAsync(productId, 1);

        var response = await client.PutAsync($"/Order/{order.Id}/restore", null);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "insufficient_stock");
        Assert.True(await IsOrderDeletedAsync(order.Id));
        Assert.Equal(1, await TestHelpers.GetProductStockAsync(_factory, productId));
    }

    [Fact]
    public async Task Restore_SecondProductInsufficient_NothingChanged()
    {
        var productA = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var productB = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, productA, 2)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, productB, 3)).StatusCode);
        var create = await client.PostAsync("/Order/me/from-cart", null);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var orderId = (await System.Net.Http.Json.HttpContentJsonExtensions
            .ReadFromJsonAsync<VTInformatica.DTOs.Order.GetOrderDto>(create.Content))!.Id;
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync($"/Order/{orderId}/delete", null)).StatusCode);
        await SetStockAsync(productB, 1);

        var response = await client.PutAsync($"/Order/{orderId}/restore", null);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "insufficient_stock");
        Assert.Equal(10, await TestHelpers.GetProductStockAsync(_factory, productA));
        Assert.Equal(1, await TestHelpers.GetProductStockAsync(_factory, productB));
        Assert.True(await IsOrderDeletedAsync(orderId));
    }

    private async Task SetStockAsync(int productId, int quantity)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Products.Where(p => p.Id == productId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, quantity));
    }

    private async Task<bool> IsOrderDeletedAsync(int orderId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId)).IsDeleted;
    }
}
