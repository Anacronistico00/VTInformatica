using System.ComponentModel.DataAnnotations;

namespace VTInformatica.Models
{
    public class Manufacturer
    {
        [Key]
        public int ManufacturerId { get; set; }

        [Required]
        public string ManufacturerName { get; set; } = string.Empty;

        [Required]
        public string ManufacturerLogo { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new List<Product>();
    }
}