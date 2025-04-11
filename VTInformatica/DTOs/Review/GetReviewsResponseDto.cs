using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Review
{
    public class GetReviewsResponseDto
    {
        [Required]
        public string Message { get; set; }

        [Required]
        public ICollection<GetReviewDto> Reviews { get; set; }
    }
}
