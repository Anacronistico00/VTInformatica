using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>
/// OrderController (AUDIT #11): errori come ProblemDetails + code, liste vuote come 200 [].
/// </summary>
[Collection(ApiCollection.Name)]
public class OrderErrorTests
{
    private readonly VtWebApplicationFactory _factory;

    public OrderErrorTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateFromCart_ProductDeletedAfterAdd_Returns404ProductNotFound()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, productId, quantity: 1)).StatusCode);
        // Soft delete scritto direttamente (senza la pulizia dei carrelli del delete via API):
        // simula un'eliminazione concorrente tra aggiunta al carrello e checkout.
        await SetProductDeletedAsync(productId);

        var response = await client.PostAsync("/Order/me/from-cart", null);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "product_not_found");
    }

    [Fact]
    public async Task CreateFromCart_InsufficientStock_Returns409InsufficientStock()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, productId, quantity: 5)).StatusCode);
        await SetStockAsync(productId, 2);

        var response = await client.PostAsync("/Order/me/from-cart", null);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "insufficient_stock");
    }

    [Fact]
    public async Task GetOrdersByEmail_NoOrders_Returns200EmptyList()
    {
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));

        await ProblemAssert.IsEmptyListAsync(await client.GetAsync("/Order/me"));
    }

    [Fact]
    public async Task GetAll_NoOrders_Returns200EmptyList()
    {
        // DB vuoto: factory dedicata, non quella condivisa (che contiene ordini di altri test).
        await using var emptyDb = new VtWebApplicationFactory();
        var admin = TestHelpers.AuthorizedClient(emptyDb,
            await TestHelpers.RegisterAndLoginAsAdminAsync(emptyDb, TestHelpers.UniqueEmail()));

        await ProblemAssert.IsEmptyListAsync(await admin.GetAsync("/Order"));
    }

    [Fact]
    public async Task Get_UnknownId_Returns404NotFound()
    {
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

        await ProblemAssert.IsProblemAsync(await admin.GetAsync("/Order/999999"), HttpStatusCode.NotFound, "not_found");
    }

    [Theory]
    [InlineData("delete")]
    [InlineData("restore")]
    public async Task SoftDeleteOrRestore_UnknownOrder_Returns404NotFound(string action)
    {
        var client = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail()));

        var response = await client.PutAsync($"/Order/999999/{action}", null);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "not_found");
    }

    private async Task SetProductDeletedAsync(int productId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Products.Where(p => p.Id == productId).ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true));
    }

    private async Task SetStockAsync(int productId, int quantity)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Products.Where(p => p.Id == productId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, quantity));
    }
}

/// <summary>Guasto del DB al checkout: 500 nel formato unico, senza dettagli.</summary>
public class OrderUnhandledErrorTests : IClassFixture<FaultInjectionFactory>
{
    private readonly FaultInjectionFactory _factory;

    public OrderUnhandledErrorTests(FaultInjectionFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateFromCart_DbFailure_Returns500InternalErrorWithoutDetail()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, productId, quantity: 1)).StatusCode);

        HttpResponseMessage response;
        _factory.CartItemDeleteFailure.Enable();
        try
        {
            response = await client.PostAsync("/Order/me/from-cart", null);
        }
        finally
        {
            _factory.CartItemDeleteFailure.Disable();
        }

        var json = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.InternalServerError, "internal_error");
        Assert.False(json.TryGetProperty("detail", out _));
    }
}
