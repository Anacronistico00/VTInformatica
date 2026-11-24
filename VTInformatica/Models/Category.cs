using System.ComponentModel.DataAnnotations;

namespace VTInformatica.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<SubCategory>? SubCategories { get; set; }
    }
}
