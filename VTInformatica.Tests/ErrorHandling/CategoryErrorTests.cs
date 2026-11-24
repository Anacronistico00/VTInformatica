using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Models;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>
/// CategoryController senza try/catch (AUDIT #11). Prima di toglierli, guardie sul CRUD
/// (nessun test copriva le categorie); poi delete con dipendenze → 409 resource_in_use.
/// </summary>
[Collection(ApiCollection.Name)]
public class CategoryErrorTests
{
    private readonly VtWebApplicationFactory _factory;

    public CategoryErrorTests(VtWebApplicationFactory factory) => _factory = factory;

    // ---- Guardie: il comportamento di successo non cambia ----

    [Fact]
    public async Task Crud_WithoutDependents_Works()
    {
        var admin = await AdminClientAsync();
        var anonymous = _factory.CreateClient();

        var create = await admin.PostAsJsonAsync("/Category", new { name = $"Categoria {Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync($"/Category/{id}")).StatusCode);
        var all = await anonymous.GetFromJsonAsync<JsonElement>("/Category");
        Assert.Contains(all.EnumerateArray(), c => c.GetProperty("id").GetInt32() == id);

        Assert.Equal(HttpStatusCode.NoContent, (await admin.PutAsJsonAsync($"/Category/{id}", new { name = "Rinominata" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/Category/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/Category/{id}")).StatusCode);
    }

    [Fact]
    public async Task UpdateOrDelete_UnknownId_Returns404NotFound()
    {
        // Guardia: NotFound() senza corpo è già ProblemDetails.
        var admin = await AdminClientAsync();

        await ProblemAssert.IsProblemAsync(await admin.PutAsJsonAsync("/Category/999999", new { name = "x" }), HttpStatusCode.NotFound, "not_found");
        await ProblemAssert.IsProblemAsync(await admin.DeleteAsync("/Category/999999"), HttpStatusCode.NotFound, "not_found");
    }

    // ---- Errori nel formato unico ----

    [Fact]
    public async Task Get_UnknownId_Returns404NotFound()
    {
        var response = await _factory.CreateClient().GetAsync("/Category/999999");

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task Delete_WithSubCategories_Returns409ResourceInUse_AndKeepsCategory()
    {
        var categoryId = await SeedCategoryWithSubCategoryAsync();
        var admin = await AdminClientAsync();

        var response = await admin.DeleteAsync($"/Category/{categoryId}");

        var json = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "resource_in_use");
        Assert.Contains("eliminati", json.GetProperty("detail").GetString());
        Assert.Equal(HttpStatusCode.OK, (await _factory.CreateClient().GetAsync($"/Category/{categoryId}")).StatusCode);
    }

    private async Task<int> SeedCategoryWithSubCategoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = new Category { Name = $"Categoria con dipendenze {Guid.NewGuid():N}" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        db.SubCategories.Add(new SubCategory { Name = "Sottocategoria collegata", CategoryId = category.Id });
        await db.SaveChangesAsync();
        return category.Id;
    }

    private async Task<HttpClient> AdminClientAsync() =>
        TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));
}
