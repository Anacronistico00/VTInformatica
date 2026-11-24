using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Models;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>
/// ManufacturerController e ManufacturerService (AUDIT #11): CRUD, errori nel formato unico e
/// delete con prodotti → 409.
/// </summary>
[Collection(ApiCollection.Name)]
public class ManufacturerErrorTests
{
    private readonly VtWebApplicationFactory _factory;

    public ManufacturerErrorTests(VtWebApplicationFactory factory) => _factory = factory;

    // ---- Guardie: il comportamento di successo non cambia ----

    [Fact]
    public async Task Crud_WithoutProducts_Works()
    {
        var admin = await AdminClientAsync();
        var anonymous = _factory.CreateClient();
        var name = $"Produttore {Guid.NewGuid():N}";

        var create = await admin.PostAsJsonAsync("/Manufacturer", new { manufacturerName = name, manufacturerLogo = "https://example.test/logo.svg" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("manufacturerId").GetInt32();

        Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync($"/Manufacturer/{id}")).StatusCode);
        // GET /Manufacturer restituisce { message, manufacturers }: contratto di successo invariato.
        var all = await anonymous.GetFromJsonAsync<JsonElement>("/Manufacturer");
        Assert.Contains(all.GetProperty("manufacturers").EnumerateArray(), m => m.GetProperty("manufacturerId").GetInt32() == id);

        Assert.Equal(HttpStatusCode.NoContent,
            (await admin.PutAsJsonAsync($"/Manufacturer/{id}", new { manufacturerId = id, manufacturerName = "Rinominato", manufacturerLogo = "https://example.test/logo2.svg" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/Manufacturer/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/Manufacturer/{id}")).StatusCode);
    }

    // ---- Errori nel formato unico ----

    [Fact]
    public async Task UnknownId_GetUpdateDelete_Return404NotFound()
    {
        var admin = await AdminClientAsync();

        await ProblemAssert.IsProblemAsync(await admin.GetAsync("/Manufacturer/999999"), HttpStatusCode.NotFound, "not_found");
        await ProblemAssert.IsProblemAsync(
            await admin.PutAsJsonAsync("/Manufacturer/999999", new { manufacturerId = 999999, manufacturerName = "x", manufacturerLogo = "x" }),
            HttpStatusCode.NotFound, "not_found");
        await ProblemAssert.IsProblemAsync(await admin.DeleteAsync("/Manufacturer/999999"), HttpStatusCode.NotFound, "not_found");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)] // solo prodotti eliminati: contano lo stesso (storico ordini)
    public async Task Delete_WithProducts_Returns409ResourceInUse_AndKeepsManufacturer(bool productDeleted)
    {
        var manufacturerId = await SeedManufacturerWithProductAsync(productDeleted);
        var admin = await AdminClientAsync();

        var response = await admin.DeleteAsync($"/Manufacturer/{manufacturerId}");

        var json = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "resource_in_use");
        Assert.Contains("eliminati", json.GetProperty("detail").GetString());
        Assert.Equal(HttpStatusCode.OK, (await _factory.CreateClient().GetAsync($"/Manufacturer/{manufacturerId}")).StatusCode);
    }

    private async Task<int> SeedManufacturerWithProductAsync(bool productDeleted)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var manufacturer = new Manufacturer { ManufacturerName = $"Produttore con prodotti {Guid.NewGuid():N}", ManufacturerLogo = "https://example.test/logo.svg" };
        db.Manufacturers.Add(manufacturer);
        await db.SaveChangesAsync();
        db.Products.Add(new Product
        {
            Name = "Prodotto collegato",
            Description = "Creato dai test",
            Price = 10m,
            Quantity = 1,
            SubCategoryId = 1,
            ManufacturerId = manufacturer.ManufacturerId,
            IsDeleted = productDeleted,
        });
        await db.SaveChangesAsync();
        return manufacturer.ManufacturerId;
    }

    private async Task<HttpClient> AdminClientAsync() =>
        TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));
}
