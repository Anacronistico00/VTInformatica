using VTInformatica.DTOs.Review;

namespace VTInformatica.Interfaces
{
    public interface IReviewService
    {
        Task<List<GetReviewDto>> GetAllAsync();
        Task<GetReviewDto> GetByIdAsync(int id);
        Task<List<GetReviewDto>> GetByEmailAsync(string email);
        Task<GetReviewDto> CreateAsync(CreateReviewDto dto);
        Task<bool> UpdateAsync(int id, UpdateReviewDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
