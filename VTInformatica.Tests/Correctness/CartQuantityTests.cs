using System.Net;
using System.Net.Http.Json;
using VTInformatica.DTOs.Cart;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// AUDIT #8: le quantità non valide o oltre lo stock disponibile vanno rifiutate
/// con un errore chiaro (400/404/409), mai accettate né trasformate in 500.
/// </summary>
[Collection(ApiCollection.Name)]
public class CartQuantityTests
{
    private readonly VtWebApplicationFactory _factory;

    public CartQuantityTests(VtWebApplicationFactory factory) => _factory = factory;

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddItem_NonPositiveQuantity_Returns400(int quantity)
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (email, client) = await NewUserAsync();

        var response = await TestHelpers.AddToCartAsync(client, email, productId, quantity);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await GetCartItemsAsync(client, email));
    }

    [Fact]
    public async Task AddItem_QuantityAboveStock_Returns409()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 5);
        var (email, client) = await NewUserAsync();

        var response = await TestHelpers.AddToCartAsync(client, email, productId, quantity: 6);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(await GetCartItemsAsync(client, email));
    }

    [Fact]
    public async Task AddItem_SumWithExistingExceedsStock_Returns409()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 5);
        var (email, client) = await NewUserAsync();
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, email, productId, quantity: 3)).StatusCode);

        var response = await TestHelpers.AddToCartAsync(client, email, productId, quantity: 3);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(3, Assert.Single(await GetCartItemsAsync(client, email)).Quantity);
    }

    [Fact]
    public async Task AddItem_QuantityEqualToStock_Returns200()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 5);
        var (email, client) = await NewUserAsync();

        var response = await TestHelpers.AddToCartAsync(client, email, productId, quantity: 5);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(5, Assert.Single(await GetCartItemsAsync(client, email)).Quantity);
    }

    [Fact]
    public async Task AddItem_NonexistentProduct_Returns404()
    {
        var (email, client) = await NewUserAsync();

        var response = await TestHelpers.AddToCartAsync(client, email, productId: int.MaxValue, quantity: 1);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<(string Email, HttpClient Client)> NewUserAsync()
    {
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        return (email, client);
    }

    private static async Task<ICollection<GetCartItemDto>> GetCartItemsAsync(HttpClient client, string email)
    {
        var cart = await client.GetFromJsonAsync<GetCartDto>($"/Cart/{email}");
        return cart!.Items;
    }
}
