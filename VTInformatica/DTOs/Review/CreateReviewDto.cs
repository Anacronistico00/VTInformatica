using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Review
{
    public class CreateReviewDto
    {
        [Range(1, 5, ErrorMessage = "Il voto deve essere compreso tra 1 e 5.")]
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; } = null!;
        // Ignorati: l'identità viene sempre dal JWT (ReviewController.Create).
        // Restano facoltativi per compatibilità con il FE che li invia.
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public int ProductId { get; set; }
    }
}
