namespace VTInformatica.DTOs.Review
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
    }
}
