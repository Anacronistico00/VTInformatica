using System.Net;
using System.Net.Http.Json;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>
/// ProductController senza try/catch (AUDIT #11): riferimenti inesistenti → 400 (non 500),
/// errori come ProblemDetails + code, liste vuote come 200 [].
/// </summary>
[Collection(ApiCollection.Name)]
public class ProductErrorTests
{
    // Id che nessun test crea (quelli di HasData e dei test sul DB condiviso possono avere prodotti,
    // a seconda dell'ordine di esecuzione): l'elenco è sempre vuoto e la risposta resta 200 [].
    private const int UnusedCategoryId = 999_001;
    private const int UnusedSubCategoryId = 999_002;
    private const int UnusedManufacturerId = 999_003;

    private readonly VtWebApplicationFactory _factory;

    public ProductErrorTests(VtWebApplicationFactory factory) => _factory = factory;

    // ---- Riferimenti inesistenti in create/update ----

    [Fact]
    public async Task Create_UnknownSubCategory_Returns400SubCategoryNotFound()
    {
        var admin = await AdminClientAsync();

        var response = await admin.PostAsJsonAsync("/Product", ProductBody(subCategoryId: 999999));

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "subcategory_not_found");
    }

    [Fact]
    public async Task Update_UnknownSubCategory_Returns400SubCategoryNotFound()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 5);
        var admin = await AdminClientAsync();

        var response = await admin.PutAsJsonAsync($"/Product/{productId}", ProductBody(subCategoryId: 999999));

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "subcategory_not_found");
    }

    [Fact]
    public async Task Create_UnknownManufacturer_Returns400ManufacturerNotFound()
    {
        var admin = await AdminClientAsync();

        var response = await admin.PostAsJsonAsync("/Product", ProductBody(manufacturerId: 999999));

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "manufacturer_not_found");
    }

    [Fact]
    public async Task Update_UnknownManufacturer_Returns400ManufacturerNotFound()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 5);
        var admin = await AdminClientAsync();

        var response = await admin.PutAsJsonAsync($"/Product/{productId}", ProductBody(manufacturerId: 999999));

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "manufacturer_not_found");
    }

    // ---- Casi già gestiti: stesso status, formato unico ----

    [Fact]
    public async Task Update_StaleExpectedQuantity_Returns409StockConflict()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var admin = await AdminClientAsync();

        var response = await admin.PutAsJsonAsync($"/Product/{productId}", ProductBody(expectedQuantity: 3));

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "stock_conflict");
    }

    [Fact]
    public async Task GetProduct_UnknownId_Returns404NotFound()
    {
        var response = await _factory.CreateClient().GetAsync("/Product/999999");

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task Search_BlankQuery_Returns400ValidationFailed()
    {
        // Guardia: una query vuota o di soli spazi diventa null nel binding e la rifiuta già la
        // validazione di [ApiController] (query è una stringa non-nullable), prima del controller.
        var response = await _factory.CreateClient().GetAsync("/Product/search?query=%20%20");

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");
    }

    // ---- Liste vuote: 200 [] ----

    [Fact]
    public async Task Search_NoResults_Returns200EmptyList()
    {
        var response = await _factory.CreateClient().GetAsync($"/Product/search?query=nessun-risultato-{Guid.NewGuid():N}");

        await ProblemAssert.IsEmptyListAsync(response);
    }

    [Theory]
    [InlineData("category", UnusedCategoryId)]
    [InlineData("subcategory", UnusedSubCategoryId)]
    [InlineData("manufacturer", UnusedManufacturerId)]
    public async Task GetByReference_NoProducts_Returns200EmptyList(string reference, int id)
    {
        var response = await _factory.CreateClient().GetAsync($"/Product/{reference}/{id}");

        await ProblemAssert.IsEmptyListAsync(response);
    }

    [Fact]
    public async Task GetAll_NoProducts_Returns200EmptyList()
    {
        // DB vuoto: factory dedicata (quella condivisa contiene i prodotti degli altri test).
        await using var emptyDb = new VtWebApplicationFactory();

        await ProblemAssert.IsEmptyListAsync(await emptyDb.CreateClient().GetAsync("/Product"));
    }

    private async Task<HttpClient> AdminClientAsync() =>
        TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

    private static object ProductBody(int subCategoryId = 1, int manufacturerId = 1, int? expectedQuantity = null) => new
    {
        name = $"Prodotto errori {Guid.NewGuid():N}",
        manufacturerId,
        description = "Prodotto creato dai test",
        price = 10m,
        quantity = 5,
        expectedQuantity,
        fullDescription = "Descrizione completa",
        subCategoryId,
        productImages = new[] { new { imageUrl = "https://example.test/img.png" } }
    };
}
