namespace VTInformatica.DTOs.Review
{
    public class CreateReviewDto
    {
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; }
        // Ignorati: l'identità viene sempre dal JWT (ReviewController.Create).
        // Restano facoltativi per compatibilità con il FE che li invia.
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public int ProductId { get; set; }
    }
}
