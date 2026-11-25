using System.Net;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>CartController senza try/catch (AUDIT #11): errori come ProblemDetails + code, stessi status.</summary>
[Collection(ApiCollection.Name)]
public class CartErrorTests
{
    private readonly VtWebApplicationFactory _factory;

    public CartErrorTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task AddItem_UnknownProduct_Returns404ProductNotFound()
    {
        var (email, client) = await UserAsync();

        var response = await TestHelpers.AddToCartAsync(client, productId: 999999, quantity: 1);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "product_not_found");
    }

    [Fact]
    public async Task AddItem_OverStock_Returns409InsufficientStock()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 2);
        var (email, client) = await UserAsync();

        var response = await TestHelpers.AddToCartAsync(client, productId, quantity: 3);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "insufficient_stock");
    }

    [Fact]
    public async Task RemoveItemQuantity_ZeroQuantity_Returns400InvalidQuantity()
    {
        var (_, client) = await UserAsync();

        var response = await client.PatchAsync("/Cart/1/delete/byquantity?quantityToRemove=0", null);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "invalid_quantity");
    }

    [Fact]
    public async Task RemoveItemQuantity_UnknownItem_Returns404NotFound()
    {
        var (_, client) = await UserAsync();

        var response = await client.PatchAsync("/Cart/999999/delete/byquantity?quantityToRemove=1", null);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task RemoveItem_UnknownItem_Returns404NotFound()
    {
        // Guardia: NotFound() senza corpo è già ProblemDetails (ApiController + ApiProblems.Customize).
        var (_, client) = await UserAsync();

        var response = await client.DeleteAsync("/Cart/999999/delete");

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "not_found");
    }

    private async Task<(string Email, HttpClient Client)> UserAsync()
    {
        var email = TestHelpers.UniqueEmail();
        return (email, TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email)));
    }
}
