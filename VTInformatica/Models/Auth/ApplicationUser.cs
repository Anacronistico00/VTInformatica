using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace VTInformatica.Models.Auth
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        public required DateOnly BirthDate { get; set; }

        [Required]
        public required bool IsActive { get; set; }

        // Valorizzati solo quando IsActive è false, azzerati alla riattivazione.
        public DeactivationReason? DeactivationReason { get; set; }

        // Obbligatoria per la sospensione, facoltativa quando il cliente cancella il proprio account.
        // Mai restituita al cliente.
        [MaxLength(500)]
        public string? DeactivationNote { get; set; }

        // UTC. Null per gli utenti disattivati prima della migration AccountDeactivation.
        public DateTime? DeactivatedAt { get; set; }

        public Cart? Cart { get; set; }

        public ICollection<ApplicationUserRole> ApplicationUserRoles { get; set; } = new List<ApplicationUserRole>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
