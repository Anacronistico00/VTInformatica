using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>
/// ReviewController (AUDIT #11): update di una review altrui → 403, delete di una review altrui → 404
/// (non ne riveliamo l'esistenza), errori nel formato unico.
/// </summary>
[Collection(ApiCollection.Name)]
public class ReviewErrorTests
{
    private readonly VtWebApplicationFactory _factory;

    public ReviewErrorTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Delete_UnknownReview_Returns404NotFound()
    {
        var client = await UserClientAsync();

        await ProblemAssert.IsProblemAsync(await client.DeleteAsync("/Review/999999"), HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task Delete_OtherUsersReview_Returns404NotFound_AndKeepsReview()
    {
        var reviewId = await CreateReviewAsync(await UserClientAsync());
        var other = await UserClientAsync();

        await ProblemAssert.IsProblemAsync(await other.DeleteAsync($"/Review/{reviewId}"), HttpStatusCode.NotFound, "not_found");

        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/Review/{reviewId}")).StatusCode);
    }

    [Fact]
    public async Task UpdateOrGet_UnknownReview_Returns404NotFound()
    {
        // Guardia: NotFound() senza corpo è già ProblemDetails.
        var client = await UserClientAsync();
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

        await ProblemAssert.IsProblemAsync(
            await client.PutAsJsonAsync("/Review/999999", new { reviewRating = 3, reviewComment = "x" }), HttpStatusCode.NotFound, "not_found");
        await ProblemAssert.IsProblemAsync(await admin.GetAsync("/Review/999999"), HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task Crud_OwnReview_Works()
    {
        // Guardia: creazione, modifica e cancellazione della propria review non cambiano.
        var client = await UserClientAsync();
        var reviewId = await CreateReviewAsync(client);

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PutAsJsonAsync($"/Review/{reviewId}", new { reviewRating = 2, reviewComment = "Modificata" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/Review/{reviewId}")).StatusCode);
    }

    private async Task<HttpClient> UserClientAsync() =>
        TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail()));

    private async Task<int> CreateReviewAsync(HttpClient client)
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var response = await client.PostAsJsonAsync("/Review", new { reviewRating = 4, reviewComment = "Recensione di prova", productId });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("reviewId").GetInt32();
    }
}
