using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.SubCategory
{
    public class SubCategoryDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
