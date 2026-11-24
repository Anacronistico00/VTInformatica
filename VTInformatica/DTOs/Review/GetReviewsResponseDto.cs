using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Review
{
    public class GetReviewsResponseDto
    {
        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public ICollection<GetReviewDto> Reviews { get; set; } = new List<GetReviewDto>();
    }
}
