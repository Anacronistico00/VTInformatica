using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.DTOs.Cart;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Characterization;

/// <summary>
/// Controllo di proprietà sul carrello (audit #2, esteso al carrello in M1c).
/// Scenario: A ha nel carrello 3 pezzi di un prodotto, B è un altro utente autenticato.
/// </summary>
[Collection(ApiCollection.Name)]
[Trait("Category", "Characterization")]
public class CartOwnershipTests
{
    private const int InitialQuantity = 3;

    private readonly VtWebApplicationFactory _factory;

    public CartOwnershipTests(VtWebApplicationFactory factory) => _factory = factory;

    // ---- Casi malevoli: B sul carrello di A ----

    [Fact]
    public async Task GetCart_OtherUsersCart_Returns403()
    {
        // AUDIT #2 risolto in M1c: l'email della route deve coincidere con quella del JWT.
        var (emailA, _, _) = await CreateUserWithCartItemAsync();
        var clientB = await CreateOtherUserClientAsync();

        var response = await clientB.GetAsync($"/Cart/{emailA}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddItem_OtherUsersCart_Returns403()
    {
        // AUDIT #2 risolto in M1c: B non può aggiungere articoli al carrello di A.
        var (emailA, _, _) = await CreateUserWithCartItemAsync();
        var otherProductId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var clientB = await CreateOtherUserClientAsync();

        var response = await TestHelpers.AddToCartAsync(clientB, emailA, otherProductId, quantity: 5);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var itemsA = await GetCartItemsAsync(emailA);
        var item = Assert.Single(itemsA);
        Assert.Equal(InitialQuantity, item.Quantity);
    }

    [Fact]
    public async Task ClearCart_OtherUsersCart_Returns403()
    {
        // AUDIT #2 risolto in M1c: B non può svuotare il carrello di A.
        var (emailA, _, _) = await CreateUserWithCartItemAsync();
        var clientB = await CreateOtherUserClientAsync();

        var response = await clientB.DeleteAsync($"/Cart/{emailA}/clear");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var item = Assert.Single(await GetCartItemsAsync(emailA));
        Assert.Equal(InitialQuantity, item.Quantity);
    }

    [Fact]
    public async Task RemoveItem_OtherUsersItem_Returns404()
    {
        // AUDIT #2 risolto in M1c: l'item deve appartenere al carrello di chi chiama.
        // 404 e non 403, per non rivelare l'esistenza dell'item.
        var (_, _, itemIdA) = await CreateUserWithCartItemAsync();
        var emailB = TestHelpers.UniqueEmail();
        var clientB = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, emailB));
        var cartsBefore = await CountCartsAsync();

        var response = await clientB.DeleteAsync($"/Cart/{itemIdA}/delete");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(InitialQuantity, await GetItemQuantityAsync(itemIdA));

        // B non ha un carrello: il controllo di proprietà (GetCartByEmailAsync) non deve crearne uno a DB.
        Assert.Equal(cartsBefore, await CountCartsAsync());
        Assert.False(await CartExistsAsync(emailB));
    }

    [Fact]
    public async Task RemoveItemQuantity_OtherUsersItem_Returns404()
    {
        // AUDIT #2 risolto in M1c: stesso controllo di proprietà di RemoveItem.
        var (_, _, itemIdA) = await CreateUserWithCartItemAsync();
        var clientB = await CreateOtherUserClientAsync();

        var response = await clientB.PatchAsync($"/Cart/{itemIdA}/delete/byquantity?quantityToRemove=1", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(InitialQuantity, await GetItemQuantityAsync(itemIdA));
    }

    // ---- Controlli positivi: l'utente sul proprio carrello ----

    [Fact]
    public async Task RemoveItem_OwnItem_Returns204()
    {
        var (emailA, clientA, itemIdA) = await CreateUserWithCartItemAsync();

        var response = await clientA.DeleteAsync($"/Cart/{itemIdA}/delete");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await GetCartItemsAsync(emailA));
    }

    [Fact]
    public async Task RemoveItemQuantity_OwnItem_Returns200()
    {
        var (_, clientA, itemIdA) = await CreateUserWithCartItemAsync();

        var response = await clientA.PatchAsync($"/Cart/{itemIdA}/delete/byquantity?quantityToRemove=1", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(InitialQuantity - 1, await GetItemQuantityAsync(itemIdA));
    }

    [Fact]
    public async Task ClearCart_OwnCart_Returns204()
    {
        var (emailA, clientA, _) = await CreateUserWithCartItemAsync();

        var response = await clientA.DeleteAsync($"/Cart/{emailA}/clear");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await GetCartItemsAsync(emailA));
    }

    // ---- Helper locali ----

    /// <summary>Crea A con un carrello contenente un prodotto (quantità InitialQuantity).</summary>
    private async Task<(string Email, HttpClient Client, int ItemId)> CreateUserWithCartItemAsync()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        var add = await TestHelpers.AddToCartAsync(client, email, productId, InitialQuantity);
        Assert.Equal(HttpStatusCode.OK, add.StatusCode);

        var cart = await client.GetFromJsonAsync<GetCartDto>($"/Cart/{email}");
        return (email, client, Assert.Single(cart!.Items).Id);
    }

    private async Task<HttpClient> CreateOtherUserClientAsync()
    {
        var token = await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail());
        return TestHelpers.AuthorizedClient(_factory, token);
    }

    private async Task<List<VTInformatica.Models.CartItem>> GetCartItemsAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cart = await db.Carts.AsNoTracking().Include(c => c.Items).SingleAsync(c => c.Email == email);
        return cart.Items.ToList();
    }

    private async Task<int?> GetItemQuantityAsync(int itemId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var item = await db.CartItems.AsNoTracking().SingleOrDefaultAsync(i => i.Id == itemId);
        return item?.Quantity;
    }

    private async Task<int> CountCartsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Carts.CountAsync();
    }

    private async Task<bool> CartExistsAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Carts.AnyAsync(c => c.Email == email);
    }
}
