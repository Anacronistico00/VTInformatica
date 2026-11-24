using System.ComponentModel.DataAnnotations.Schema;
using VTInformatica.DTOs.Account;
using VTInformatica.DTOs.Product;

namespace VTInformatica.DTOs.Review
{
    public class GetReviewDto
    {
        public int ReviewId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ProductId { get; set; }
    }
}
