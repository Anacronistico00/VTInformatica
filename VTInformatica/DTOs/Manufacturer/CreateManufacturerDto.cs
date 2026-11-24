using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Manufacturer
{
    public class CreateManufacturerDto
    {
        [Required]
        public string ManufacturerName { get; set; } = null!;
        [Required]
        public string ManufacturerLogo { get; set; } = null!;
    }
}
