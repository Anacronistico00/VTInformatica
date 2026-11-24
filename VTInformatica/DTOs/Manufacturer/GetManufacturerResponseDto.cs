using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Manufacturer
{
    public class GetManufacturerResponseDto
    {
        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public ICollection<ManufacturerDto> Manufacturers { get; set; } = new List<ManufacturerDto>();
    }
}
