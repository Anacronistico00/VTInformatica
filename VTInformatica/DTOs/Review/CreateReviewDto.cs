namespace VTInformatica.DTOs.Review
{
    public class CreateReviewDto
    {
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public int ProductId { get; set; }
    }
}
