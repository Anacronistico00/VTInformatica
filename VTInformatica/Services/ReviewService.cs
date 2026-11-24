using Microsoft.EntityFrameworkCore;
using VTInformatica.Data;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.Review;
using VTInformatica.Exceptions;
using VTInformatica.Interfaces;
using VTInformatica.Mapping;
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
                .Select(ReviewMapper.Projection)
                .ToListAsync();
        }

        public async Task<GetReviewDto?> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Where(r => r.ReviewId == id)
                .Select(ReviewMapper.Projection)
                .FirstOrDefaultAsync();
        }

        public async Task<List<GetReviewDto>> GetByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return new List<GetReviewDto>();

            var reviews = await _context.Reviews
                .Where(r => r.Email == user.Email)
                .Select(ReviewMapper.Projection)
                .ToListAsync();

            return reviews;
        }

        public async Task<GetReviewDto> CreateAsync(CreateReviewDto dto)
        {
            // Prodotto inesistente o eliminato (il query filter lo esclude): 404 invece della violazione FK (500).
            if (!await _context.Products.AnyAsync(p => p.Id == dto.ProductId))
                throw new ProductNotFoundException(dto.ProductId);

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

            return (await GetByIdAsync(review.ReviewId))!;
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
