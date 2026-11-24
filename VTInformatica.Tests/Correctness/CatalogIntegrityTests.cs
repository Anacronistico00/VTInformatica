using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VTInformatica.Models;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// Sottocategoria con categoria inesistente → 400; categoria del prodotto ricavata dalla
/// sottocategoria (una copia sul prodotto si disallineerebbe quando la sottocategoria cambia categoria).
/// </summary>
[Collection(ApiCollection.Name)]
public class CatalogIntegrityTests
{
    private const int KnownCategoryId = 6;

    private readonly VtWebApplicationFactory _factory;

    public CatalogIntegrityTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task SubCategory_Create_UnknownCategory_Returns400CategoryNotFound()
    {
        var admin = await AdminClientAsync();

        var response = await admin.PostAsJsonAsync("/SubCategory", new { name = NewName(), categoryId = 999999 });

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "category_not_found");
    }

    [Fact]
    public async Task SubCategory_Update_UnknownCategory_Returns400CategoryNotFound_NothingChanged()
    {
        var admin = await AdminClientAsync();
        var id = await CreateSubCategoryAsync(admin, KnownCategoryId);

        var response = await admin.PutAsJsonAsync($"/SubCategory/{id}", new { name = NewName(), categoryId = 999999 });

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "category_not_found");
        var current = await admin.GetFromJsonAsync<JsonElement>($"/SubCategory/{id}");
        Assert.Equal(KnownCategoryId, current.GetProperty("categoryId").GetInt32());
    }

    [Fact]
    public async Task MovingASubCategory_MovesItsProductsToTheNewCategory()
    {
        var admin = await AdminClientAsync();
        var categoryA = await CreateCategoryAsync(admin);
        var categoryB = await CreateCategoryAsync(admin);
        var subCategoryId = await CreateSubCategoryAsync(admin, categoryA);
        var productId = await CreateProductAsync(admin, subCategoryId);

        var move = await admin.PutAsJsonAsync($"/SubCategory/{subCategoryId}", new { name = NewName(), categoryId = categoryB });
        Assert.Equal(HttpStatusCode.NoContent, move.StatusCode);

        Assert.Contains(productId, await ProductIdsOfCategoryAsync(categoryB));
        Assert.DoesNotContain(productId, await ProductIdsOfCategoryAsync(categoryA));
        var product = await _factory.CreateClient().GetFromJsonAsync<JsonElement>($"/Product/{productId}");
        Assert.Equal(categoryB, product.GetProperty("category").GetProperty("id").GetInt32());
    }

    [Fact]
    public void Product_HasNoCategoryIdOfItsOwn()
    {
        // La categoria è un dato della sottocategoria: una seconda copia sul prodotto può solo disallinearsi.
        Assert.Null(typeof(Product).GetProperty("CategoryId"));
        Assert.Null(typeof(Product).GetProperty("Category"));
    }

    private async Task<HttpClient> AdminClientAsync() =>
        TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

    private static string NewName() => $"Nome {Guid.NewGuid():N}";

    private static async Task<int> CreateCategoryAsync(HttpClient admin)
    {
        var create = await admin.PostAsJsonAsync("/Category", new { name = NewName() });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        return (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task<int> CreateSubCategoryAsync(HttpClient admin, int categoryId)
    {
        var create = await admin.PostAsJsonAsync("/SubCategory", new { name = NewName(), categoryId });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        return (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task<int> CreateProductAsync(HttpClient admin, int subCategoryId)
    {
        var create = await admin.PostAsJsonAsync("/Product", new
        {
            name = NewName(),
            manufacturerId = 1,
            description = "Prodotto dei test",
            price = 10m,
            quantity = 5,
            fullDescription = "Descrizione completa",
            subCategoryId,
            productImages = new[] { new { imageUrl = "https://img.test.local/a.png" } }
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        return int.Parse(create.Headers.Location!.Segments.Last());
    }

    private async Task<List<int>> ProductIdsOfCategoryAsync(int categoryId)
    {
        var list = await _factory.CreateClient().GetFromJsonAsync<JsonElement>($"/Product/category/{categoryId}");
        return list.EnumerateArray().Select(p => p.GetProperty("id").GetInt32()).ToList();
    }
}
