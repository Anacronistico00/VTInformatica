using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.DTOs.Cart;
using VTInformatica.DTOs.Order;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Integrity;

/// <summary>
/// Eliminare un prodotto non deve cancellare lo storico ordini: il prodotto sparisce dal
/// catalogo e dai carrelli, gli ordini restano.
/// </summary>
[Collection(ApiCollection.Name)]
public class ProductSoftDeleteTests
{
    private readonly VtWebApplicationFactory _factory;

    public ProductSoftDeleteTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task DeleteProduct_KeepsOrderHistory()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10, price: 80m);
        var (email, token, _) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 2);

        await DeleteProductAsAdminAsync(productId);

        var client = TestHelpers.AuthorizedClient(_factory, token);
        var orders = await client.GetFromJsonAsync<List<GetOrderDto>>($"/Order/user/{email}");

        var item = Assert.Single(Assert.Single(orders!).Items);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(80m, item.Product.Price);
        Assert.StartsWith("Prodotto test", item.Product.Name);
    }

    [Fact]
    public async Task DeleteProduct_HidesFromCatalog()
    {
        // Guardia: vero anche con l'hard delete, deve restare vero con il soft delete.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var client = _factory.CreateClient();
        var name = (await GetJsonAsync(client, $"/Product/{productId}")).GetProperty("name").GetString();

        await DeleteProductAsAdminAsync(productId);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/Product/{productId}")).StatusCode);
        Assert.DoesNotContain(productId, await GetIdsAsync(client, "/Product"));

        // La search risponde 404 quando non trova nulla (contratto attuale).
        var search = await client.GetAsync($"/Product/search?query={Uri.EscapeDataString(name!)}");
        if (search.StatusCode != HttpStatusCode.NotFound)
        {
            Assert.Equal(HttpStatusCode.OK, search.StatusCode);
            var ids = (await search.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray()
                .Select(p => p.GetProperty("id").GetInt32());
            Assert.DoesNotContain(productId, ids);
        }
    }

    [Fact]
    public async Task DeleteProduct_RemovesItFromCarts_AndTheTotalExcludesIt()
    {
        var deletedId = await TestHelpers.SeedProductAsync(_factory, quantity: 10, price: 100m);
        var keptId = await TestHelpers.SeedProductAsync(_factory, quantity: 10, price: 30m);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, email, deletedId, quantity: 1)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, email, keptId, quantity: 2)).StatusCode);

        await DeleteProductAsAdminAsync(deletedId);

        var cart = await client.GetFromJsonAsync<GetCartDto>($"/Cart/{email}");
        Assert.Equal(2, Assert.Single(cart!.Items).Quantity);
        Assert.Equal(60m, cart.TotalPrice);
    }

    [Fact]
    public async Task CancelOrder_WithDeletedProduct_Succeeds_AndKeepsItems()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (email, token, order) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 3);

        await DeleteProductAsAdminAsync(productId);

        var client = TestHelpers.AuthorizedClient(_factory, token);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync($"/Order/{order.Id}/delete", null)).StatusCode);

        var orders = await client.GetFromJsonAsync<List<GetOrderDto>>($"/Order/user/{email}");
        var cancelled = Assert.Single(orders!);
        Assert.True(cancelled.IsDeleted);
        Assert.Equal(3, Assert.Single(cancelled.Items).Quantity);
    }

    private async Task DeleteProductAsAdminAsync(int productId)
    {
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/Product/{productId}")).StatusCode);
    }

    private static async Task<JsonElement> GetJsonAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<List<int>> GetIdsAsync(HttpClient client, string url) =>
        (await GetJsonAsync(client, url)).EnumerateArray().Select(p => p.GetProperty("id").GetInt32()).ToList();
}
