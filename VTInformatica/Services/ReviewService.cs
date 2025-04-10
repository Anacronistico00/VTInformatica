using Microsoft.EntityFrameworkCore;
using VTInformatica.Data;
using VTInformatica.DTOs.Review;
using VTInformatica.Interfaces;
using VTInformatica.Models;

namespace VTInformatica.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReviewDto>> GetAllAsync()
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    ReviewRating = r.ReviewRating,
                    ReviewComment = r.ReviewComment,
                    CreatedAt = r.CreatedAt,
                    UserId = r.UserId,
                    ProductId = r.ProductId,
                })
                .ToListAsync();
        }

        public async Task<ReviewDto> GetByIdAsync(int id)
        {
            var r = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (r == null) return null;

            return new ReviewDto
            {
                ReviewId = r.ReviewId,
                ReviewRating = r.ReviewRating,
                ReviewComment = r.ReviewComment,
                CreatedAt = r.CreatedAt,
                UserId = r.UserId,
                ProductId = r.ProductId,
            };
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto dto)
        {
            var review = new Review
            {
                ReviewRating = dto.ReviewRating,
                ReviewComment = dto.ReviewComment,
                UserId = dto.UserId,
                ProductId = dto.ProductId,
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(review.ReviewId);
        }

        public async Task<bool> UpdateAsync(int id, UpdateReviewDto dto, string userId)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            if (review.UserId != userId)
            {
                return false;
            }

            review.ReviewRating = dto.ReviewRating;
            review.ReviewComment = dto.ReviewComment;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int reviewId, string loggedUserId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;

            if (review.UserId != loggedUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this review.");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
