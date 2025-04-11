using System.ComponentModel.DataAnnotations.Schema;
using VTInformatica.DTOs.Account;
using VTInformatica.DTOs.Product;

namespace VTInformatica.DTOs.Review
{
    public class GetReviewDto
    {
        public int ReviewId { get; set; }
        public string CustomerEmail { get; set; }
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductId { get; set; }
    }
}
