using System.ComponentModel.DataAnnotations;
using VTInformatica.DTOs.SubCategory;

namespace VTInformatica.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<GetSubCategoryDto>? SubCategories { get; set; }
    }
}
