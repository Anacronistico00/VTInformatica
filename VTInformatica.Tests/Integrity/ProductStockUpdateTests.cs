using System.Net;
using System.Net.Http.Json;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Integrity;

/// <summary>
/// Lost update dello stock: l'admin salva il form con la Quantity letta prima delle vendite
/// avvenute nel frattempo. Con ExpectedQuantity (lo stock che l'admin vedeva) l'update è
/// condizionale: se lo stock è cambiato risponde 409 e non tocca niente.
/// ExpectedQuantity è opzionale per compatibilità con i client che non lo inviano.
/// </summary>
[Collection(ApiCollection.Name)]
public class ProductStockUpdateTests
{
    private readonly VtWebApplicationFactory _factory;

    public ProductStockUpdateTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Update_StaleExpectedQuantity_Returns409_AndKeepsStock()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var admin = await AdminClientAsync();

        // Vendita avvenuta dopo che l'admin ha aperto il form (stock 10 → 7).
        await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 3);

        var response = await admin.PutAsJsonAsync($"/Product/{productId}",
            ProductBody(quantity: 10, expectedQuantity: 10, name: "Nome aggiornato"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(7, await TestHelpers.GetProductStockAsync(_factory, productId));

        // Tutto o niente: neanche gli altri campi vengono salvati.
        var product = await _factory.CreateClient().GetFromJsonAsync<System.Text.Json.JsonElement>($"/Product/{productId}");
        Assert.NotEqual("Nome aggiornato", product.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Update_MatchingExpectedQuantity_Returns204_AndSetsStock()
    {
        // Guardia: se lo stock non è cambiato l'update va a buon fine.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var admin = await AdminClientAsync();

        var response = await admin.PutAsJsonAsync($"/Product/{productId}",
            ProductBody(quantity: 25, expectedQuantity: 10));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(25, await TestHelpers.GetProductStockAsync(_factory, productId));
    }

    [Fact]
    public async Task Update_WithoutExpectedQuantity_KeepsCurrentBehavior()
    {
        // Guardia di compatibilità: senza ExpectedQuantity la Quantity viene scritta senza controllo.
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var admin = await AdminClientAsync();

        var response = await admin.PutAsJsonAsync($"/Product/{productId}",
            ProductBody(quantity: 4, expectedQuantity: null));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(4, await TestHelpers.GetProductStockAsync(_factory, productId));
    }

    private async Task<HttpClient> AdminClientAsync() =>
        TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

    private static object ProductBody(int quantity, int? expectedQuantity, string? name = null) => new
    {
        name = name ?? $"Prodotto stock {Guid.NewGuid():N}",
        manufacturerId = 1,
        description = "Prodotto creato dai test",
        price = 100m,
        quantity,
        expectedQuantity,
        fullDescription = "Descrizione completa",
        subCategoryId = 1,
        productImages = new[] { new { imageUrl = "https://example.test/img.png" } }
    };
}
