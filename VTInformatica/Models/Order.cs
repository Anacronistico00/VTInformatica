using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VTInformatica.Models.Auth;

namespace VTInformatica.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        public string CustomerEmail { get; set; }

        public string CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public ApplicationUser Customer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CustomerComment { get; set; }

        public bool IsDeleted { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
