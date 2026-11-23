using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// AUDIT #7: ordine, righe, scalatura stock e svuotamento carrello sono un'unica
/// operazione: se una parte fallisce non deve restare nulla.
/// </summary>
public class OrderAtomicityTests : IClassFixture<FaultInjectionFactory>
{
    private readonly FaultInjectionFactory _factory;

    public OrderAtomicityTests(FaultInjectionFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateFromCart_FailureWhileClearingCart_RollsBackEverything()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, email, productId, quantity: 3)).StatusCode);

        HttpResponseMessage response;
        _factory.CartItemDeleteFailure.Enable();
        try
        {
            response = await client.PostAsync($"/Order/from-cart/{email}", null);
        }
        finally
        {
            _factory.CartItemDeleteFailure.Disable();
        }

        // Il 500 generico resta (gestione errori: audit #11, M3); qui conta lo stato.
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(await db.Orders.AnyAsync(o => o.CustomerEmail == email), "Nessun ordine deve restare salvato.");
        Assert.Equal(10, await TestHelpers.GetProductStockAsync(_factory, productId));
        var cart = await db.Carts.AsNoTracking().Include(c => c.Items).SingleAsync(c => c.Email == email);
        Assert.Equal(3, Assert.Single(cart.Items).Quantity);
    }
}
