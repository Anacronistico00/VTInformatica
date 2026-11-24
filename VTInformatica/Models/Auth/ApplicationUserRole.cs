using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace VTInformatica.Models.Auth
{
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public new Guid UserId { get; set; }
        public new Guid RoleId { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
        [ForeignKey("RoleId")]
        public ApplicationRole Role { get; set; } = null!;
    }
}
