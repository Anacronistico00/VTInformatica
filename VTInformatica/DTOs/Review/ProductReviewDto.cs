namespace VTInformatica.DTOs.Review
{
    /// <summary>Review dentro le risposte pubbliche dei prodotti: senza l'email dell'autore.</summary>
    public class ProductReviewDto
    {
        public int ReviewId { get; set; }
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
