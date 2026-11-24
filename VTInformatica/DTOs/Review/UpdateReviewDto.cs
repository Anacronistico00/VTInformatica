using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Review
{
    public class UpdateReviewDto
    {
        [Range(1, 5, ErrorMessage = "Il voto deve essere compreso tra 1 e 5.")]
        public int ReviewRating { get; set; }
        public string ReviewComment { get; set; }
    }
}
