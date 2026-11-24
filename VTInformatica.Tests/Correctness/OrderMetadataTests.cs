using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.DTOs.Cart;
using VTInformatica.DTOs.Order;
using VTInformatica.Models;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>CustomerComment scelto dal cliente (nessun testo fisso) e OrderNumber unique.</summary>
[Collection(ApiCollection.Name)]
public class OrderMetadataTests
{
    private readonly VtWebApplicationFactory _factory;

    public OrderMetadataTests(VtWebApplicationFactory factory) => _factory = factory;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateFromCart_StoresTheCommentSentByTheCustomer(bool useMeRoute)
    {
        var (client, email) = await NewUserWithCartAsync();

        var response = useMeRoute
            ? await client.PostAsJsonAsync("/Order/me/from-cart", new { customerComment = "Citofono rotto, chiamare" })
            : await client.PostAsJsonAsync($"/Order/from-cart/{email}", new { customerComment = "Citofono rotto, chiamare" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<GetOrderDto>();
        Assert.Equal("Citofono rotto, chiamare", order!.CustomerComment);
    }

    [Fact]
    public async Task CreateFromCart_WithoutBody_HasNoComment()
    {
        var (client, _) = await NewUserWithCartAsync();

        var response = await client.PostAsync("/Order/me/from-cart", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<GetOrderDto>();
        Assert.Null(order!.CustomerComment);
    }

    [Fact]
    public async Task CreateFromCart_CommentTooLong_Returns400_NothingOrdered()
    {
        var (client, email) = await NewUserWithCartAsync();

        var response = await client.PostAsJsonAsync("/Order/me/from-cart", new { customerComment = new string('x', 501) });

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");
        var cart = await (await client.GetAsync("/Cart/me")).Content.ReadFromJsonAsync<GetCartDto>();
        Assert.Single(cart!.Items);
    }

    [Fact]
    public void OrderNumber_HasAUniqueIndex()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var index = db.Model.FindEntityType(typeof(Order))!
            .GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Order.OrderNumber) }));

        Assert.NotNull(index);
        Assert.True(index!.IsUnique);
    }

    private async Task<(HttpClient Client, string Email)> NewUserWithCartAsync()
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 10);
        var email = TestHelpers.UniqueEmail();
        var client = TestHelpers.AuthorizedClient(_factory, await TestHelpers.RegisterAndLoginAsync(_factory, email));
        Assert.Equal(HttpStatusCode.OK, (await TestHelpers.AddToCartAsync(client, email, productId, 1)).StatusCode);
        return (client, email);
    }
}
