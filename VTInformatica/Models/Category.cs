using System.ComponentModel.DataAnnotations;

namespace VTInformatica.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Product>? Products { get; set; }

        public ICollection<SubCategory>? SubCategories { get; set; }
    }
}
