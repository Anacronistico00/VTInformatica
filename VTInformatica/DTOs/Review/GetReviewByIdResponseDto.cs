using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Review
{
    public class GetReviewByIdResponseDto
    {
        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public GetReviewDto Review { get; set; } = new GetReviewDto();
    }
}
