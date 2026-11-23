using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.DTOs.Order;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// AUDIT #6: le righe d'ordine devono conservare il prezzo unitario al momento
/// dell'acquisto, non leggere il prezzo corrente del prodotto.
/// </summary>
[Collection(ApiCollection.Name)]
public class OrderPriceTests
{
    private readonly VtWebApplicationFactory _factory;

    public OrderPriceTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetOrdersByEmail_AfterPriceChange_ShowsPriceAtPurchase()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10, price: 100m);
        var (email, token, _) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 1);
        await SetProductPriceAsync(productId, 150m);
        var client = TestHelpers.AuthorizedClient(_factory, token);

        var orders = await client.GetFromJsonAsync<List<GetOrderDto>>($"/Order/user/{email}");

        var item = Assert.Single(Assert.Single(orders!).Items);
        Assert.Equal(100m, item.Product.Price);
    }

    [Fact]
    public async Task GetById_AsAdmin_AfterPriceChange_ShowsPriceAtPurchase()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10, price: 100m);
        var (_, _, order) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 1);
        await SetProductPriceAsync(productId, 150m);
        var adminClient = TestHelpers.AuthorizedClient(
            _factory, await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

        var response = await adminClient.GetAsync($"/Order/{order.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetOrderDto>();
        Assert.Equal(100m, Assert.Single(fetched!.Items).Product.Price);
    }

    private async Task SetProductPriceAsync(int productId, decimal price)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = await db.Products.FindAsync(productId);
        product!.Price = price;
        await db.SaveChangesAsync();
    }
}
