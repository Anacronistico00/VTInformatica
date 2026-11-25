using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;

namespace VTInformatica.IntegrationTests;

/// <summary>
/// Checkout paralleli sullo stesso prodotto su SQL Server vero: l'UPDATE condizionale atomico
/// (WHERE Quantity >= qty) deve impedire l'overselling. Su SQLite le richieste non sono davvero parallele.
/// </summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public class StockRaceTests
{
    private readonly SqlServerFixture _sql;

    public StockRaceTests(SqlServerFixture sql) => _sql = sql;

    [Fact]
    public async Task ParallelCheckouts_NeverOversell_ExactlyStockOverQuantitySucceed()
    {
        const int stock = 7, quantity = 2, buyers = 12;
        const int expectedSuccesses = stock / quantity; // 3, ne avanza 1

        await using var api = await ApiFactory.CreateAsync(_sql);
        var productId = await api.SeedProductAsync(stock);

        var clients = new List<HttpClient>();
        for (var i = 0; i < buyers; i++)
        {
            var client = api.ClientFor(await api.RegisterAndLoginAsync(ApiFactory.UniqueEmail()));
            Assert.Equal(HttpStatusCode.OK, (await ApiFactory.AddToCartAsync(client, productId, quantity)).StatusCode);
            clients.Add(client);
        }

        // Tutti i checkout partono insieme.
        var start = new TaskCompletionSource();
        var requests = clients.Select(async client =>
        {
            await start.Task;
            return await client.PostAsync("/Order/me/from-cart", null);
        }).ToList();
        start.SetResult();
        var responses = await Task.WhenAll(requests);

        var created = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflicts = responses.Where(r => r.StatusCode == HttpStatusCode.Conflict).ToList();
        Assert.Equal(expectedSuccesses, created);
        Assert.Equal(buyers - expectedSuccesses, conflicts.Count);
        foreach (var conflict in conflicts)
        {
            var problem = await conflict.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("insufficient_stock", problem.GetProperty("code").GetString());
        }

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var remaining = (await db.Products.AsNoTracking().SingleAsync(p => p.Id == productId)).Quantity;
        Assert.Equal(stock - expectedSuccesses * quantity, remaining);
        Assert.True(remaining >= 0);
        Assert.Equal(expectedSuccesses, await db.Orders.CountAsync(o => o.OrderItems.Any(i => i.ProductId == productId)));
    }
}
