using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.DTOs.Review;
using VTInformatica.Models;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Security;

/// <summary>
/// Review: l'identità arriva sempre dal JWT (UserId/Email del body ignorati);
/// update/delete solo sulle proprie review, altrimenti 404.
/// Status code attuali del controller: create 201, update 204, delete 200.
/// </summary>
[Collection(ApiCollection.Name)]
public class ReviewOwnershipTests
{
    private readonly VtWebApplicationFactory _factory;

    public ReviewOwnershipTests(VtWebApplicationFactory factory) => _factory = factory;

    // ---- Create ----

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);

        var response = await _factory.CreateClient().PostAsJsonAsync("/Review", new
        {
            reviewRating = 5,
            reviewComment = "Anonima",
            productId
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_BodyImpersonatesOtherUser_SavesTokenIdentity()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var a = await CreateUserAsync();
        var b = await CreateUserAsync();

        // A prova a scrivere la review a nome di B.
        var response = await a.Client.PostAsJsonAsync("/Review", new
        {
            reviewRating = 1,
            reviewComment = "Impersonificazione",
            userId = b.Id,
            email = b.Email,
            productId
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetReviewDto>();
        var saved = await GetReviewAsync(created!.ReviewId);
        Assert.Equal(a.Id, saved!.UserId);
        Assert.Equal(a.Email, saved.Email);
    }

    [Fact]
    public async Task Create_WithoutIdentityInBody_Returns201WithTokenIdentity()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var a = await CreateUserAsync();

        var response = await a.Client.PostAsJsonAsync("/Review", new
        {
            reviewRating = 4,
            reviewComment = "Senza identità nel body",
            productId
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetReviewDto>();
        var saved = await GetReviewAsync(created!.ReviewId);
        Assert.Equal(a.Id, saved!.UserId);
        Assert.Equal(a.Email, saved.Email);
    }

    // ---- Update ----

    [Fact]
    public async Task Update_OwnReview_Returns204()
    {
        var a = await CreateUserAsync();
        var reviewId = await CreateOwnReviewAsync(a);

        var response = await a.Client.PutAsJsonAsync($"/Review/{reviewId}", new { reviewRating = 2, reviewComment = "Modificata" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Modificata", (await GetReviewAsync(reviewId))!.ReviewComment);
    }

    [Fact]
    public async Task Update_OtherUsersReview_Returns404()
    {
        var a = await CreateUserAsync();
        var b = await CreateUserAsync();
        var reviewIdB = await CreateOwnReviewAsync(b);

        var response = await a.Client.PutAsJsonAsync($"/Review/{reviewIdB}", new { reviewRating = 1, reviewComment = "Manomessa" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Originale", (await GetReviewAsync(reviewIdB))!.ReviewComment);
    }

    // ---- Delete ----

    [Fact]
    public async Task Delete_OwnReview_Returns200()
    {
        var a = await CreateUserAsync();
        var reviewId = await CreateOwnReviewAsync(a);

        var response = await a.Client.DeleteAsync($"/Review/{reviewId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(await GetReviewAsync(reviewId));
    }

    [Fact]
    public async Task Delete_OtherUsersReview_Returns404()
    {
        // 404 e non 403: non riveliamo l'esistenza della review di un altro utente.
        var a = await CreateUserAsync();
        var b = await CreateUserAsync();
        var reviewIdB = await CreateOwnReviewAsync(b);

        var response = await a.Client.DeleteAsync($"/Review/{reviewIdB}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(await GetReviewAsync(reviewIdB));
    }

    // ---- Helper locali ----

    private async Task<(string Email, string Id, HttpClient Client)> CreateUserAsync()
    {
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        return (email, await TestHelpers.GetUserIdAsync(_factory, email), client);
    }

    /// <summary>
    /// Crea una review con commento "Originale" come fa il FE oggi (identità propria anche nel body),
    /// così l'helper funziona sia prima sia dopo il fix.
    /// </summary>
    private async Task<int> CreateOwnReviewAsync((string Email, string Id, HttpClient Client) user)
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var response = await user.Client.PostAsJsonAsync("/Review", new
        {
            reviewRating = 5,
            reviewComment = "Originale",
            userId = user.Id,
            email = user.Email,
            productId
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<GetReviewDto>())!.ReviewId;
    }

    private async Task<Review?> GetReviewAsync(int reviewId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Reviews.AsNoTracking().SingleOrDefaultAsync(r => r.ReviewId == reviewId);
    }
}
