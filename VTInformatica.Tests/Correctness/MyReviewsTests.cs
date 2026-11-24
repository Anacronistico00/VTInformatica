using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// GET /Review/me: le recensioni dell'utente del token, per permettere al FE di offrire
/// modifica ed eliminazione solo sulle proprie (le review nei prodotti non hanno l'autore).
/// </summary>
[Collection(ApiCollection.Name)]
public class MyReviewsTests
{
    private readonly VtWebApplicationFactory _factory;

    public MyReviewsTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/Review/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task NoReviews_Returns200EmptyList()
    {
        var client = await NewUserClientAsync();

        await ProblemAssert.IsEmptyListAsync(await client.GetAsync("/Review/me"));
    }

    [Fact]
    public async Task ReturnsOnlyTheOwnReviews_NewestFirst_WithoutEmail()
    {
        var productA = await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var productB = await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var me = await NewUserClientAsync();
        var other = await NewUserClientAsync();
        var first = await CreateReviewAsync(me, productA, 4, "Prima");
        await CreateReviewAsync(other, productA, 2, "Di un altro");
        var second = await CreateReviewAsync(me, productB, 5, "Seconda");

        var json = await me.GetFromJsonAsync<JsonElement>("/Review/me");

        var reviews = json.EnumerateArray().ToList();
        Assert.Equal(new[] { second, first }, reviews.Select(r => r.GetProperty("reviewId").GetInt32()));
        var newest = reviews[0];
        Assert.Equal(productB, newest.GetProperty("productId").GetInt32());
        Assert.Equal(5, newest.GetProperty("reviewRating").GetInt32());
        Assert.Equal("Seconda", newest.GetProperty("reviewComment").GetString());
        Assert.True(newest.TryGetProperty("createdAt", out _));
        Assert.False(newest.TryGetProperty("customerEmail", out _));
    }

    private async Task<HttpClient> NewUserClientAsync() =>
        TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail()));

    private static async Task<int> CreateReviewAsync(HttpClient client, int productId, int rating, string comment)
    {
        var create = await client.PostAsJsonAsync("/Review", new { reviewRating = rating, reviewComment = comment, productId });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        return (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("reviewId").GetInt32();
    }
}
