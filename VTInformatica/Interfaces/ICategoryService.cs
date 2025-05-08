using VTInformatica.DTOs.Category;

namespace VTInformatica.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CategoryDto dto);
        Task<bool> UpdateAsync(int id, EditCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
