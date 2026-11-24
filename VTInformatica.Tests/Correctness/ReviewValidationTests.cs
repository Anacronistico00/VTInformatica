using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// M3c2: review con prodotto inesistente → 404 (era 500), rating validato (1–5),
/// email dell'autore non esposta nelle risposte pubbliche dei prodotti.
/// </summary>
[Collection(ApiCollection.Name)]
public class ReviewValidationTests
{
    private readonly VtWebApplicationFactory _factory;

    public ReviewValidationTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_UnknownProduct_Returns404ProductNotFound()
    {
        var client = await NewUserClientAsync();

        var response = await client.PostAsJsonAsync("/Review", Body(productId: 999999, rating: 4));

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "product_not_found");
    }

    [Fact]
    public async Task Create_DeletedProduct_Returns404ProductNotFound()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/Product/{productId}")).StatusCode);

        var response = await admin.PostAsJsonAsync("/Review", Body(productId, rating: 4));

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "product_not_found");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Create_RatingOutOfRange_Returns400(int rating)
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var client = await NewUserClientAsync();

        var response = await client.PostAsJsonAsync("/Review", Body(productId, rating));

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Create_RatingAtTheLimits_Returns201(int rating)
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var client = await NewUserClientAsync();

        var response = await client.PostAsJsonAsync("/Review", Body(productId, rating));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_RatingOutOfRange_Returns400_ReviewUnchanged()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var client = await NewUserClientAsync();
        var reviewId = await CreateReviewAsync(client, productId, rating: 4);

        var response = await client.PutAsJsonAsync($"/Review/{reviewId}", new { reviewRating = 9, reviewComment = "Modificata" });

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");
    }

    [Fact]
    public async Task ProductResponses_DoNotExposeTheReviewersEmail()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        await CreateReviewAsync(client, productId, rating: 5);
        var anonymous = _factory.CreateClient();

        var single = await anonymous.GetStringAsync($"/Product/{productId}");
        var list = await anonymous.GetStringAsync("/Product");

        Assert.Contains("\"reviewRating\"", single);
        Assert.DoesNotContain(email, single, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(email, list, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("customerEmail", single, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AdminReviewList_StillShowsTheEmail_ForModeration()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 1);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        var reviewId = await CreateReviewAsync(client, productId, rating: 3);
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

        var json = await admin.GetFromJsonAsync<JsonElement>($"/Review/{reviewId}");

        Assert.Equal(email, json.GetProperty("review").GetProperty("customerEmail").GetString());
    }

    private static object Body(int productId, int rating) =>
        new { reviewRating = rating, reviewComment = "Commento di prova", productId };

    private async Task<HttpClient> NewUserClientAsync() =>
        TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail()));

    private static async Task<int> CreateReviewAsync(HttpClient client, int productId, int rating)
    {
        var create = await client.PostAsJsonAsync("/Review", Body(productId, rating));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        return (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("reviewId").GetInt32();
    }
}
