using System.Net;
using System.Net.Http.Json;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Integrity;

/// <summary>
/// Create/Update prodotto: stock negativo e prezzo non positivo vanno rifiutati con 400
/// (prima solo [Required], che su int/decimal non valida niente).
/// </summary>
[Collection(ApiCollection.Name)]
public class ProductValidationTests
{
    private readonly VtWebApplicationFactory _factory;

    public ProductValidationTests(VtWebApplicationFactory factory) => _factory = factory;

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(5, 0)]
    [InlineData(5, -10)]
    public async Task Create_InvalidQuantityOrPrice_Returns400(int quantity, decimal price)
    {
        var admin = await AdminClientAsync();

        var response = await admin.PostAsJsonAsync("/Product", ProductBody(quantity, price));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(5, 0)]
    [InlineData(5, -10)]
    public async Task Update_InvalidQuantityOrPrice_Returns400_AndKeepsProduct(int quantity, decimal price)
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 7, price: 50m);
        var admin = await AdminClientAsync();

        var response = await admin.PutAsJsonAsync($"/Product/{productId}", ProductBody(quantity, price));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(7, await TestHelpers.GetProductStockAsync(_factory, productId));
    }

    [Fact]
    public async Task Create_ZeroQuantity_IsAllowed()
    {
        // Guardia: 0 = esaurito è un valore lecito.
        var admin = await AdminClientAsync();

        var response = await admin.PostAsJsonAsync("/Product", ProductBody(quantity: 0, price: 10m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task<HttpClient> AdminClientAsync() =>
        TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

    // Corpo valido per Create e Update (sottocategoria e produttore da HasData), salvo quantity/price.
    private static object ProductBody(int quantity, decimal price) => new
    {
        name = $"Prodotto validazione {Guid.NewGuid():N}",
        manufacturerId = 1,
        description = "Prodotto creato dai test",
        price,
        quantity,
        fullDescription = "Descrizione completa",
        subCategoryId = 1,
        productImages = new[] { new { imageUrl = "https://example.test/img.png" } }
    };
}
