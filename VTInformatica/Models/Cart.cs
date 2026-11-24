using System.ComponentModel.DataAnnotations.Schema;
using VTInformatica.Models.Auth;

namespace VTInformatica.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
