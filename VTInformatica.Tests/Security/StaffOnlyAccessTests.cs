using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Security;

/// <summary>
/// Letture per id e liste complete di ordini e recensioni: solo Admin/Seller.
/// Un Customer non legge gli ordini altrui (nemmeno per id) e lo staff può annullare e ripristinare ordini altrui.
/// </summary>
[Collection(ApiCollection.Name)]
public class StaffOnlyAccessTests
{
    private readonly VtWebApplicationFactory _factory;

    public StaffOnlyAccessTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetOrderById_AsCustomer_OtherUsersOrder_Returns403()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, _, orderA) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 1);
        var clientB = await CustomerClientAsync();

        var response = await clientB.GetAsync($"/Order/{orderA.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAllOrders_AsCustomer_Returns403()
    {
        var response = await (await CustomerClientAsync()).GetAsync("/Order");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/Review")]
    [InlineData("/Review/1")]
    public async Task ReviewReads_AsCustomer_Return403(string url)
    {
        var response = await (await CustomerClientAsync()).GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Staff_CancelAndRestoreOtherUsersOrder_MovesStockBackAndForth()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var (_, _, orderA) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 2);
        var admin = TestHelpers.AuthorizedClient(_factory,
            await TestHelpers.RegisterAndLoginAsAdminAsync(_factory, TestHelpers.UniqueEmail()));

        var cancel = await admin.PutAsync($"/Order/{orderA.Id}/delete", null);

        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);
        Assert.Equal(10, await TestHelpers.GetProductStockAsync(_factory, productId));

        var restore = await admin.PutAsync($"/Order/{orderA.Id}/restore", null);

        Assert.Equal(HttpStatusCode.OK, restore.StatusCode);
        Assert.Equal(8, await TestHelpers.GetProductStockAsync(_factory, productId));
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False((await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderA.Id)).IsDeleted);
    }

    private async Task<HttpClient> CustomerClientAsync() =>
        TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, TestHelpers.UniqueEmail()));
}
