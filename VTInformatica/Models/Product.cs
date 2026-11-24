using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VTInformatica.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public int ManufacturerId { get; set; }

        [ForeignKey("ManufacturerId")]
        public Manufacturer? Manufacturer { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        // Soft delete: il prodotto sparisce dal catalogo (query filter) ma resta per lo storico ordini.
        public bool IsDeleted { get; set; }

        public string? FullDescription { get; set; }

        // La categoria è quella della sottocategoria (SubCategory.CategoryId): nessuna seconda copia.
        public int SubCategoryId { get; set; }
        [ForeignKey("SubCategoryId")]
        public SubCategory? SubCategory { get; set; }
        public ICollection<ProductImage>? ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<CartItem>? CartItems { get; set; }
        public ICollection<OrderItem>? Orders { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
