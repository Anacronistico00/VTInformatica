using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Category
{
    public class CategoryDto
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
