using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Models;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>
/// SubCategoryController (5 catch) e SubCategoryService (5 log + rethrow) senza try/catch (AUDIT #11).
/// Prima guardie sul CRUD (nessun test copriva le sottocategorie), poi delete con prodotti → 409.
/// </summary>
[Collection(ApiCollection.Name)]
public class SubCategoryErrorTests
{
    // Categoria da HasData usata solo per le sottocategorie create da questi test.
    private const int CategoryId = 6;

    private readonly VtWebApplicationFactory _factory;

    public SubCategoryErrorTests(VtWebApplicationFactory factory) => _factory = factory;

    // ---- Guardie: il comportamento di successo non cambia ----

    [Fact]
    public async Task Crud_WithoutProducts_Works()
    {
        var admin = await AdminClientAsync();
        var anonymous = _factory.CreateClient();

        var create = await admin.PostAsJsonAsync("/SubCategory", new { name = $"Sottocategoria {Guid.NewGuid():N}", categoryId = CategoryId });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync($"/SubCategory/{id}")).StatusCode);
        var all = await anonymous.GetFromJsonAsync<JsonElement>("/SubCategory");
        Assert.Contains(all.EnumerateArray(), s => s.GetProperty("id").GetInt32() == id);

        Assert.Equal(HttpStatusCode.NoContent,
            (await admin.PutAsJsonAsync($"/SubCategory/{id}", new { name = "Rinominata", categoryId = CategoryId })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/SubCategory/{id}")).StatusCode);
        await ProblemAssert.IsProblemAsync(await anonymous.GetAsync($"/SubCategory/{id}"), HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task UpdateOrDelete_UnknownId_Returns404NotFound()
    {
        var admin = await AdminClientAsync();

        await ProblemAssert.IsProblemAsync(
            await admin.PutAsJsonAsync("/SubCategory/999999", new { name = "x", categoryId = CategoryId }), HttpStatusCode.NotFound, "not_found");
        await ProblemAssert.IsProblemAsync(await admin.DeleteAsync("/SubCategory/999999"), HttpStatusCode.NotFound, "not_found");
    }

    // ---- Delete con prodotti collegati ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)] // solo prodotti eliminati: contano lo stesso (storico ordini)
    public async Task Delete_WithProducts_Returns409ResourceInUse_AndKeepsSubCategory(bool productDeleted)
    {
        var subCategoryId = await SeedSubCategoryWithProductAsync(productDeleted);
        var admin = await AdminClientAsync();

        var response = await admin.DeleteAsync($"/SubCategory/{subCategoryId}");

        var json = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "resource_in_use");
        Assert.Contains("eliminati", json.GetProperty("detail").GetString());
        Assert.Equal(HttpStatusCode.OK, (await _factory.CreateClient().GetAsync($"/SubCategory/{subCategoryId}")).StatusCode);
    }

    private async Task<int> SeedSubCategoryWithProductAsync(bool productDeleted)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subCategory = new SubCategory { Name = $"Sottocategoria con prodotti {Guid.NewGuid():N}", CategoryId = CategoryId };
        db.SubCategories.Add(subCategory);
        await db.SaveChangesAsync();
        db.Products.Add(new Product
        {
            Name = "Prodotto collegato",
            Description = "Creato dai test",
            Price = 10m,
            Quantity = 1,
            CategoryId = CategoryId,
            SubCategoryId = subCategory.Id,
            ManufacturerId = 1,
            IsDeleted = productDeleted,
        });
        await db.SaveChangesAsync();
        return subCategory.Id;
    }

    private async Task<HttpClient> AdminClientAsync() =>
        TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));
}
