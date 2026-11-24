namespace VTInformatica.DTOs.Review
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public int ProductId { get; set; }
    }
}
