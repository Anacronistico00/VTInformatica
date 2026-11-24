using System.Linq.Expressions;
using VTInformatica.DTOs.Review;
using VTInformatica.Models;

namespace VTInformatica.Mapping
{
    public static class ReviewMapper
    {
        // Vista per Admin/Seller (moderazione): include l'email dell'autore.
        public static readonly Expression<Func<Review, GetReviewDto>> Projection = r => new GetReviewDto
        {
            ReviewId = r.ReviewId,
            CustomerEmail = r.Email,
            ReviewRating = r.ReviewRating,
            ReviewComment = r.ReviewComment,
            CreatedAt = r.CreatedAt,
            ProductId = r.ProductId,
        };

        // Vista dell'autore stesso: nessuna email.
        public static readonly Expression<Func<Review, MyReviewDto>> MineProjection = r => new MyReviewDto
        {
            ReviewId = r.ReviewId,
            ProductId = r.ProductId,
            ReviewRating = r.ReviewRating,
            ReviewComment = r.ReviewComment,
            CreatedAt = r.CreatedAt,
        };
    }
}
