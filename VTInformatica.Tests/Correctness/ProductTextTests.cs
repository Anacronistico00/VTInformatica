using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>Testi del prodotto senza spazi ai bordi; titolo della documentazione Swagger.</summary>
[Collection(ApiCollection.Name)]
public class ProductTextTests
{
    private readonly VtWebApplicationFactory _factory;

    public ProductTextTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_TrimsNameDescriptionAndFullDescription()
    {
        var admin = await AdminClientAsync();

        var create = await admin.PostAsJsonAsync("/Product", Body("   Intel Core i9   ", "  Processore  ", "  Descrizione completa \t"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = int.Parse(create.Headers.Location!.Segments.Last());

        var product = await _factory.CreateClient().GetFromJsonAsync<JsonElement>($"/Product/{id}");
        Assert.Equal(
            ("Intel Core i9", "Processore", "Descrizione completa"),
            (product.GetProperty("name").GetString(), product.GetProperty("description").GetString(), product.GetProperty("fullDescription").GetString()));
    }

    [Fact]
    public async Task Update_TrimsNameDescriptionAndFullDescription()
    {
        var admin = await AdminClientAsync();
        var create = await admin.PostAsJsonAsync("/Product", Body("Nome", "Descrizione", "Completa"));
        var id = int.Parse(create.Headers.Location!.Segments.Last());

        var update = await admin.PutAsJsonAsync($"/Product/{id}", Body("  Nome nuovo ", "\tDescrizione nuova  ", " Completa nuova "));
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var product = await _factory.CreateClient().GetFromJsonAsync<JsonElement>($"/Product/{id}");
        Assert.Equal(
            ("Nome nuovo", "Descrizione nuova", "Completa nuova"),
            (product.GetProperty("name").GetString(), product.GetProperty("description").GetString(), product.GetProperty("fullDescription").GetString()));
    }

    [Theory]
    [InlineData("    ", "Descrizione")]
    [InlineData("Nome", "   ")]
    public async Task Create_BlankNameOrDescription_Returns400(string name, string description)
    {
        var admin = await AdminClientAsync();

        var response = await admin.PostAsJsonAsync("/Product", Body(name, description, "Completa"));

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");
    }

    [Fact]
    public void Swagger_TitleNamesThisApi()
    {
        var title = _factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1").Info.Title;

        Assert.Equal("VTInformatica API", title);
    }

    private async Task<HttpClient> AdminClientAsync() =>
        TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

    private static object Body(string name, string description, string fullDescription) => new
    {
        name,
        manufacturerId = 1,
        description,
        price = 10m,
        quantity = 5,
        fullDescription,
        subCategoryId = 1,
        productImages = new[] { new { imageUrl = "https://img.test.local/a.png" } }
    };
}
