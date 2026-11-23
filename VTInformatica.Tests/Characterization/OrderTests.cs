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
    public async Task CreateFromCart_EmptyCart_Returns500()
    {
        // Fotografa la gestione errori attuale: l'InvalidOperationException "carrello vuoto"
        // viene catturata dal controller e trasformata in un 500 generico (audit #11).
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        var response = await client.PostAsync($"/Order/from-cart/{email}", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
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
    public async Task GetOrdersByEmail_OtherUsersEmail_Returns200WithTheirOrders()
    {
        // AUDIT #2 (IDOR): l'email arriva dalla route e non dai claim del JWT.
        // Qualunque utente autenticato legge gli ordini di un altro.
        // Comportamento SBAGLIATO documentato: in M1 questo test deve diventare 403/404.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (emailA, _, orderA) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 1);
        var tokenB = await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail());
        var clientB = TestHelpers.AuthorizedClient(_factory, tokenB);

        var response = await clientB.GetAsync($"/Order/user/{emailA}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<List<GetOrderDto>>();
        var leaked = Assert.Single(orders!);
        Assert.Equal(emailA, leaked.CustomerEmail);
        Assert.Equal(orderA.OrderNumber, leaked.OrderNumber);
    }

    [Fact]
    public async Task SoftDelete_OtherUsersOrder_Returns200()
    {
        // AUDIT #3: nessun controllo di proprietà su PUT /Order/{id}/delete.
        // Qualunque utente autenticato cancella l'ordine di un altro (e lo stock torna a magazzino).
        // Comportamento SBAGLIATO documentato: in M1 questo test deve diventare 403/404.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, _, orderA) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 2);
        var tokenB = await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail());
        var clientB = TestHelpers.AuthorizedClient(_factory, tokenB);

        var response = await clientB.PutAsync($"/Order/{orderA.Id}/delete", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
