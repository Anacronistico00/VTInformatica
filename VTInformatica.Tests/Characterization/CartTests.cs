using System.Net;
using System.Net.Http.Json;
using VTInformatica.DTOs.Cart;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Characterization;

/// <summary>
/// Test di caratterizzazione sul carrello: descrivono il comportamento ATTUALE.
/// </summary>
[Collection(ApiCollection.Name)]
[Trait("Category", "Characterization")]
public class CartTests
{
    private readonly VtWebApplicationFactory _factory;

    public CartTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task AddItem_NewCart_CreatesCartWithItem()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10, price: 25.50m);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        var add = await TestHelpers.AddToCartAsync(client, productId, quantity: 2);

        Assert.Equal(HttpStatusCode.OK, add.StatusCode);

        var cart = await client.GetFromJsonAsync<GetCartDto>("/Cart/me");
        var item = Assert.Single(cart!.Items);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(25.50m, item.Product.Price);
        Assert.Equal(51.00m, cart.TotalPrice);
    }

    [Fact]
    public async Task AddItem_SameProductTwice_SumsQuantities()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        await TestHelpers.AddToCartAsync(client, productId, quantity: 1);
        await TestHelpers.AddToCartAsync(client, productId, quantity: 3);

        var cart = await client.GetFromJsonAsync<GetCartDto>("/Cart/me");
        var item = Assert.Single(cart!.Items);
        Assert.Equal(4, item.Quantity);
    }

    [Fact]
    public async Task AddItem_WithoutToken_Returns401()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var client = _factory.CreateClient();

        var response = await TestHelpers.AddToCartAsync(client, productId, quantity: 1);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
