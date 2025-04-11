using Microsoft.EntityFrameworkCore;
using VTInformatica.Data;
using VTInformatica.DTOs.Product;
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

        public async Task<List<GetReviewDto>> GetAllAsync()
        {
            return await _context.Reviews
                .Select(r => new GetReviewDto
                {
                    ReviewId = r.ReviewId,
                    CustomerEmail = r.Email,
                    ReviewRating = r.ReviewRating,
                    ReviewComment = r.ReviewComment,
                    CreatedAt = r.CreatedAt,
                    ProductId = r.ProductId,
                })
                .ToListAsync();
        }

        public async Task<GetReviewDto> GetByIdAsync(int id)
        {
            var r = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (r == null) return null;

            return new GetReviewDto
            {
                ReviewId = r.ReviewId,
                CustomerEmail = r.Email,
                ReviewRating = r.ReviewRating,
                ReviewComment = r.ReviewComment,
                CreatedAt = r.CreatedAt,
                ProductId = r.ProductId,
            };
        }

        public async Task<List<GetReviewDto>> GetByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return new List<GetReviewDto>();

            var reviews = await _context.Reviews
                .Where(r => r.Email == user.Email)
                .Select(r => new GetReviewDto
                {
                    ReviewId = r.ReviewId,
                    CustomerEmail = r.Email,
                    ReviewRating = r.ReviewRating,
                    ReviewComment = r.ReviewComment,
                    CreatedAt = r.CreatedAt,
                    ProductId = r.ProductId,
                })
                .ToListAsync();

            return reviews;
        }

        public async Task<GetReviewDto> CreateAsync(CreateReviewDto dto)
        {
            var review = new Review
            {
                ReviewRating = dto.ReviewRating,
                ReviewComment = dto.ReviewComment,
                UserId = dto.UserId,
                Email = dto.Email,
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
