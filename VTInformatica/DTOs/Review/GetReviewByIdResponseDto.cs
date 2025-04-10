using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Review
{
    public class GetReviewByIdResponseDto
    {
        [Required]
        public string Message { get; set; }

        [Required]
        public ReviewDto Review { get; set; }
    }
}
