using System.ComponentModel.DataAnnotations;

namespace VTInformatica.Models
{
    public class Manufacturer
    {
        [Key]
        public int ManufacturerId { get; set; }

        [Required]
        public string ManufacturerName { get; set; }

        [Required]  
        public string ManufacturerLogo { get; set; }

        public List<Product> Products { get; set; }
    }
}