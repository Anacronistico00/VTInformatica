using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.SubCategory
{
    public class SubCategoryDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public int CategoryId { get; set; }
    }
}
