using System.Net;
using System.Net.Http.Json;
using VTInformatica.DTOs.Cart;
using VTInformatica.DTOs.Order;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// Route /Cart/me e /Order/me: identità solo dal token. Le vecchie route con {email} non esistono più.
/// </summary>
[Collection(ApiCollection.Name)]
public class MeRoutesTests
{
    private readonly VtWebApplicationFactory _factory;

    public MeRoutesTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CartMe_AddGetClear_WorkOnTheTokenUsersCart()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        var add = await client.PostAsJsonAsync("/Cart/me/add", new CartItemDto { ProductId = productId, Quantity = 2 });
        Assert.Equal(HttpStatusCode.OK, add.StatusCode);

        var get = await client.GetAsync("/Cart/me");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var cart = await get.Content.ReadFromJsonAsync<GetCartDto>();
        Assert.Equal(email, cart!.Email);
        Assert.Equal(2, Assert.Single(cart.Items).Quantity);

        var clear = await client.DeleteAsync("/Cart/me/clear");
        Assert.Equal(HttpStatusCode.NoContent, clear.StatusCode);
        var after = await (await client.GetAsync("/Cart/me")).Content.ReadFromJsonAsync<GetCartDto>();
        Assert.Empty(after!.Items);
    }

    [Fact]
    public async Task CartMe_DoesNotSeeOtherUsersCart()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var emailA = TestHelpers.UniqueEmail();
        var clientA = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, emailA));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(clientA, productId, 3)).StatusCode);
        var clientB = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail()));

        var cartB = await (await clientB.GetAsync("/Cart/me")).Content.ReadFromJsonAsync<GetCartDto>();

        Assert.Empty(cartB!.Items);
    }

    [Theory]
    [InlineData("GET", "/Cart/me")]
    [InlineData("POST", "/Cart/me/add")]
    [InlineData("DELETE", "/Cart/me/clear")]
    [InlineData("GET", "/Order/me")]
    [InlineData("POST", "/Order/me/from-cart")]
    public async Task MeRoutes_WithoutToken_Return401(string method, string url)
    {
        var client = _factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrderMe_CreateFromCart_ThenList_WorkOnTheTokenUsersOrders()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/Cart/me/add",
            new CartItemDto { ProductId = productId, Quantity = 4 })).StatusCode);

        var create = await client.PostAsync("/Order/me/from-cart", null);

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var order = await create.Content.ReadFromJsonAsync<GetOrderDto>();
        Assert.Equal(email, order!.CustomerEmail);
        Assert.Equal(6, await TestHelpers.GetProductStockAsync(_factory, productId));

        var list = await (await client.GetAsync("/Order/me")).Content.ReadFromJsonAsync<List<GetOrderDto>>();
        Assert.Equal(order.OrderNumber, Assert.Single(list!).OrderNumber);
    }

    [Fact]
    public async Task OrderMe_ListsOnlyOwnOrders()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 1);
        var clientB = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail()));

        await ProblemAssert.IsEmptyListAsync(await clientB.GetAsync("/Order/me"));
    }

    [Theory]
    [InlineData("GET", "/Cart/{0}")]
    [InlineData("POST", "/Cart/{0}/add")]
    [InlineData("DELETE", "/Cart/{0}/clear")]
    [InlineData("GET", "/Order/user/{0}")]
    [InlineData("POST", "/Order/from-cart/{0}")]
    public async Task OldEmailRoutes_AreGone(string method, string urlFormat)
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        // Un carrello con un articolo: se la route esistesse, risponderebbe con un esito diverso da 404.
        await client.PostAsJsonAsync("/Cart/me/add", new CartItemDto { ProductId = productId, Quantity = 1 });

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), string.Format(urlFormat, email)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
