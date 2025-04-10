using VTInformatica.DTOs.SubCategory;

namespace VTInformatica.Interfaces
{
    public interface ISubCategoryService
    {
        Task<List<SubCategoryDto>> GetAllAsync();
        Task<SubCategoryDto> GetByIdAsync(int id);
        Task<SubCategoryDto> CreateAsync(SubCategoryDto dto);
        Task<bool> UpdateAsync(int id, SubCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
