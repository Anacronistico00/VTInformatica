using System.Net.Http.Json;
using System.Text.Json;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>AUDIT #13: l'id del produttore annidato nelle risposte prodotto è quello vero.</summary>
[Collection(ApiCollection.Name)]
public class ProductMappingTests
{
    private readonly VtWebApplicationFactory _factory;

    public ProductMappingTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetProduct_NestedManufacturerHasItsId()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 1);

        var product = await _factory.CreateClient().GetFromJsonAsync<JsonElement>($"/Product/{productId}");

        Assert.Equal(1, product.GetProperty("manufacturerId").GetInt32());
        Assert.Equal(1, product.GetProperty("manufacturer").GetProperty("manufacturerId").GetInt32());
    }

    [Fact]
    public async Task GetAllAndSearch_NestedManufacturerHasItsId()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var client = _factory.CreateClient();

        var all = await client.GetFromJsonAsync<JsonElement>("/Product");
        var fromList = all.EnumerateArray().Single(p => p.GetProperty("id").GetInt32() == productId);

        Assert.Equal(1, fromList.GetProperty("manufacturer").GetProperty("manufacturerId").GetInt32());
    }

    [Fact]
    public async Task ProductsByManufacturer_NestedManufacturerHasItsId()
    {
        await TestHelpers.SeedProductAsync(_factory, quantity: 1);

        var list = await _factory.CreateClient().GetFromJsonAsync<JsonElement>("/Product/manufacturer/1");

        Assert.All(list.EnumerateArray(), p =>
            Assert.Equal(1, p.GetProperty("manufacturer").GetProperty("manufacturerId").GetInt32()));
    }
}
