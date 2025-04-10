using VTInformatica.DTOs.Review;

namespace VTInformatica.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetAllAsync();
        Task<ReviewDto> GetByIdAsync(int id);
        Task<ReviewDto> CreateAsync(CreateReviewDto dto);
        Task<bool> UpdateAsync(int id, UpdateReviewDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
