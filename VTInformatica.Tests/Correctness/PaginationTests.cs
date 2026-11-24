using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// AUDIT #12: paginazione opzionale su GET /Product e GET /Order. Senza parametri la risposta
/// resta l'array completo; con page/pageSize è lo stesso array, ordinato per id, più X-Total-Count.
/// </summary>
[Collection(ApiCollection.Name)]
public class PaginationTests
{
    private const string TotalCountHeader = "X-Total-Count";

    private readonly VtWebApplicationFactory _factory;

    public PaginationTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Product_WithoutParameters_ReturnsEverything_WithoutTotalHeader()
    {
        for (var i = 0; i < 3; i++) await TestHelpers.SeedProductAsync(_factory, quantity: 1);

        var response = await _factory.CreateClient().GetAsync("/Product");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var all = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(all.GetArrayLength() >= 3);
        Assert.False(response.Headers.Contains(TotalCountHeader));
    }

    [Fact]
    public async Task Product_Page_ReturnsTheSliceInIdOrder_WithTheTotal()
    {
        for (var i = 0; i < 5; i++) await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var client = _factory.CreateClient();
        var allIds = Ids(await client.GetFromJsonAsync<JsonElement>("/Product")).OrderBy(id => id).ToList();

        var first = await client.GetAsync("/Product?page=1&pageSize=2");
        var second = await client.GetAsync("/Product?page=2&pageSize=2");

        Assert.Equal(allIds.Take(2), Ids(await first.Content.ReadFromJsonAsync<JsonElement>()));
        Assert.Equal(allIds.Skip(2).Take(2), Ids(await second.Content.ReadFromJsonAsync<JsonElement>()));
        Assert.Equal(allIds.Count.ToString(), first.Headers.GetValues(TotalCountHeader).Single());
        Assert.Equal(allIds.Count.ToString(), second.Headers.GetValues(TotalCountHeader).Single());
    }

    [Fact]
    public async Task Product_PageBeyondTheEnd_ReturnsEmptyList_WithTheTotal()
    {
        await TestHelpers.SeedProductAsync(_factory, quantity: 1);

        var response = await _factory.CreateClient().GetAsync("/Product?page=100000&pageSize=20");

        await ProblemAssert.IsEmptyListAsync(response);
        Assert.True(int.Parse(response.Headers.GetValues(TotalCountHeader).Single()) >= 1);
    }

    [Fact]
    public async Task Product_OnlyPageSize_StartsFromTheFirstPage()
    {
        await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var client = _factory.CreateClient();
        var firstId = Ids(await client.GetFromJsonAsync<JsonElement>("/Product")).Min();

        var response = await client.GetAsync("/Product?pageSize=1");

        Assert.Equal(new[] { firstId }, Ids(await response.Content.ReadFromJsonAsync<JsonElement>()));
    }

    [Fact]
    public async Task Product_OnlyPage_UsesTheDefaultPageSize()
    {
        for (var i = 0; i < 21; i++) await TestHelpers.SeedProductAsync(_factory, quantity: 1);

        var response = await _factory.CreateClient().GetAsync("/Product?page=1");

        Assert.Equal(20, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength());
    }

    [Theory]
    [InlineData("/Product?page=0")]
    [InlineData("/Product?page=-1")]
    [InlineData("/Product?pageSize=0")]
    [InlineData("/Product?pageSize=101")]
    [InlineData("/Product?page=abc")]
    [InlineData("/Order?page=0")]
    [InlineData("/Order?pageSize=101")]
    public async Task InvalidPagingParameters_Return400(string url)
    {
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

        var response = await admin.GetAsync(url);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");
    }

    [Fact]
    public async Task Order_Page_ReturnsOneOrder_WithTheTotal()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 1);
        await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 1);
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));
        var total = (await admin.GetFromJsonAsync<JsonElement>("/Order")).GetArrayLength();

        var response = await admin.GetAsync("/Order?page=1&pageSize=1");

        Assert.Equal(1, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength());
        Assert.Equal(total.ToString(), response.Headers.GetValues(TotalCountHeader).Single());
    }

    [Fact]
    public async Task TotalCountHeader_IsExposedToTheBrowser()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Product?page=1&pageSize=1");
        request.Headers.Add("Origin", VtWebApplicationFactory.AllowedCorsOrigin);

        var response = await _factory.CreateClient().SendAsync(request);

        Assert.Contains(TotalCountHeader, response.Headers.GetValues("Access-Control-Expose-Headers").Single());
    }

    private static List<int> Ids(JsonElement array) =>
        array.EnumerateArray().Select(p => p.GetProperty("id").GetInt32()).ToList();
}
