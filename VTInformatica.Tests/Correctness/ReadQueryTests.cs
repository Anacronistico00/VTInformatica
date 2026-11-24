using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Interfaces;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>
/// AUDIT #12: le letture non usano il change tracker (AsNoTracking) e caricano solo ciò che
/// il DTO mostra. Verificato sul tracker del DbContext dopo ogni lettura dei service.
/// </summary>
[Collection(ApiCollection.Name)]
public class ReadQueryTests
{
    private readonly VtWebApplicationFactory _factory;

    public ReadQueryTests(VtWebApplicationFactory factory) => _factory = factory;

    public static IEnumerable<object[]> Reads() => new[]
    {
        new object[] { "Product.GetAll" },
        new object[] { "Product.GetById" },
        new object[] { "Product.Search" },
        new object[] { "Product.ByCategory" },
        new object[] { "Product.BySubCategory" },
        new object[] { "Product.ByManufacturer" },
        new object[] { "Order.GetAll" },
        new object[] { "Order.GetById" },
        new object[] { "Order.GetByEmail" },
        new object[] { "Category.GetAll" },
        new object[] { "Cart.Get" },
    };

    [Theory]
    [MemberData(nameof(Reads))]
    public async Task Read_LeavesTheChangeTrackerEmpty(string read)
    {
        var productId = await TestHelpers.SeedProductAsync(_factory, quantity: 5);
        var (email, _, order) = await TestHelpers.CreateUserWithOrderAsync(_factory, productId, quantity: 1);
        using var scope = _factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<ApplicationDbContext>();

        switch (read)
        {
            case "Product.GetAll": await sp.GetRequiredService<IProductService>().GetAllAsync(); break;
            case "Product.GetById": await sp.GetRequiredService<IProductService>().GetByIdAsync(productId); break;
            case "Product.Search": await sp.GetRequiredService<IProductService>().SearchProductsAsync("Prodotto test"); break;
            case "Product.ByCategory": await sp.GetRequiredService<IProductService>().GetProductsByCategoryIdAsync(1); break;
            case "Product.BySubCategory": await sp.GetRequiredService<IProductService>().GetProductsBySubCategoryIdAsync(1); break;
            case "Product.ByManufacturer": await sp.GetRequiredService<IProductService>().GetProductsByManufacturerIdAsync(1); break;
            case "Order.GetAll": await sp.GetRequiredService<IOrderService>().GetAllAsync(); break;
            case "Order.GetById": await sp.GetRequiredService<IOrderService>().GetByIdAsync(order.Id); break;
            case "Order.GetByEmail": await sp.GetRequiredService<IOrderService>().GetOrdersByEmailAsync(email); break;
            case "Category.GetAll": await sp.GetRequiredService<ICategoryService>().GetAllAsync(); break;
            case "Cart.Get": await sp.GetRequiredService<ICartService>().GetCartByEmailAsync(email); break;
            default: throw new ArgumentException(read);
        }

        Assert.Empty(db.ChangeTracker.Entries());
    }
}
