namespace VTInformatica.DTOs.Review
{
    /// <summary>Recensione dell'utente autenticato (GET /Review/me): senza email, l'autore è chi chiama.</summary>
    public class MyReviewDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
