using VTInformatica.DTOs.Product;
using VTInformatica.Models;

namespace VTInformatica.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<List<Product>> SearchProductsAsync(string query);
        Task<Product> CreateAsync(CreateProductDto dto);
        Task<bool> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<GetProductDto>> GetProductsByCategoryIdAsync(int categoryId);
        Task<List<GetProductDto>> GetProductsBySubCategoryIdAsync(int subCategoryId);
        Task<List<GetProductDto>> GetProductsByManufacturerIdAsync(int categoryId);

    }
}
