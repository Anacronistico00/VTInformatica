using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Review
{
    public class GetReviewByIdResponseDto
    {
        [Required]
        public string Message { get; set; }

        [Required]
        public GetReviewDto Review { get; set; }
    }
}
