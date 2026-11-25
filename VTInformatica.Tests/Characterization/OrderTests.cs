using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.DTOs.Order;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Characterization;

/// <summary>
/// Test di caratterizzazione sugli ordini: descrivono il comportamento ATTUALE,
/// compresi i bug noti (vedi docs/AUDIT-prima.md).
/// </summary>
[Collection(ApiCollection.Name)]
[Trait("Category", "Characterization")]
public class OrderTests
{
    private readonly VtWebApplicationFactory _factory;

    public OrderTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateFromCart_DecrementsStock_AndEmptiesCart()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);

        var (email, _, order) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 3);

        Assert.Equal(email, order.CustomerEmail);
        var item = Assert.Single(order.Items);
        Assert.Equal(3, item.Quantity);
        Assert.False(order.IsDeleted);

        Assert.Equal(7, await TestHelpers.GetProductStockAsync(_factory, productId));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cart = await db.Carts.AsNoTracking().Include(c => c.Items).SingleAsync(c => c.Email == email);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task CreateFromCart_EmptyCart_Returns409CartEmpty()
    {
        // AUDIT #11: il carrello vuoto è un errore di dominio 409 cart_empty in ProblemDetails.
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        var response = await client.PostAsync("/Order/me/from-cart", null);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "cart_empty");
    }

    [Fact]
    public async Task SoftDelete_MarksDeleted_AndRestoresStock()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, token, order) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 3);
        Assert.Equal(7, await TestHelpers.GetProductStockAsync(_factory, productId));
        var client = TestHelpers.AuthorizedClient(_factory, token);

        var response = await client.PutAsync($"/Order/{order.Id}/delete", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(await IsOrderDeletedAsync(order.Id));
        Assert.Equal(10, await TestHelpers.GetProductStockAsync(_factory, productId));
    }

    [Fact]
    public async Task GetMyOrders_Returns200WithOwnOrders()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (emailA, tokenA, orderA) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 1);
        var clientA = TestHelpers.AuthorizedClient(_factory, tokenA);

        var response = await clientA.GetAsync("/Order/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<List<GetOrderDto>>();
        var own = Assert.Single(orders!);
        Assert.Equal(orderA.OrderNumber, own.OrderNumber);
    }

    [Fact]
    public async Task CreateFromCart_OtherUsersCartStaysUntouched()
    {
        // AUDIT #2: l'ordine parte sempre dal carrello del token: B (carrello vuoto) non tocca quello di A.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var emailA = TestHelpers.UniqueEmail();
        var clientA = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, emailA));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(clientA, productId, quantity: 2)).StatusCode);
        var tokenB = await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail());
        var clientB = TestHelpers.AuthorizedClient(_factory, tokenB);

        var response = await clientB.PostAsync("/Order/me/from-cart", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(10, await TestHelpers.GetProductStockAsync(_factory, productId));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(await db.Orders.AnyAsync(o => o.CustomerEmail == emailA));
        var cartA = await db.Carts.AsNoTracking().Include(c => c.Items).SingleAsync(c => c.Email == emailA);
        Assert.Single(cartA.Items);
    }

    [Fact]
    public async Task SoftDelete_OtherUsersOrder_Returns404()
    {
        // AUDIT #3: solo il proprietario (o Admin/Seller) può annullare l'ordine.
        // 404 e non 403, per non rivelare l'esistenza dell'ordine.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, _, orderA) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 2);
        var tokenB = await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail());
        var clientB = TestHelpers.AuthorizedClient(_factory, tokenB);

        var response = await clientB.PutAsync($"/Order/{orderA.Id}/delete", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(await IsOrderDeletedAsync(orderA.Id));
        Assert.Equal(8, await TestHelpers.GetProductStockAsync(_factory, productId));
    }

    [Fact]
    public async Task Restore_OwnDeletedOrder_Returns200()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, tokenA, orderA) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 2);
        var clientA = TestHelpers.AuthorizedClient(_factory, tokenA);
        Assert.Equal(HttpStatusCode.OK, (await clientA.PutAsync($"/Order/{orderA.Id}/delete", null)).StatusCode);

        var response = await clientA.PutAsync($"/Order/{orderA.Id}/restore", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(await IsOrderDeletedAsync(orderA.Id));
        Assert.Equal(8, await TestHelpers.GetProductStockAsync(_factory, productId));
    }

    [Fact]
    public async Task Restore_OtherUsersOrder_Returns404()
    {
        // AUDIT #3: stesso controllo di proprietà del delete, stessa risposta 404.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, tokenA, orderA) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 2);
        var clientA = TestHelpers.AuthorizedClient(_factory, tokenA);
        Assert.Equal(HttpStatusCode.OK, (await clientA.PutAsync($"/Order/{orderA.Id}/delete", null)).StatusCode);
        var tokenB = await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail());
        var clientB = TestHelpers.AuthorizedClient(_factory, tokenB);

        var response = await clientB.PutAsync($"/Order/{orderA.Id}/restore", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(await IsOrderDeletedAsync(orderA.Id));
        Assert.Equal(10, await TestHelpers.GetProductStockAsync(_factory, productId));
    }

    private async Task<bool> IsOrderDeletedAsync(int orderId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId);
        return order.IsDeleted;
    }
}
