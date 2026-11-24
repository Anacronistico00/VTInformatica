using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Account
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public DateOnly BirthDate { get; set; }

        // IsActive rimosso (AUDIT #9): lo decide il server. Se il client lo invia ancora viene ignorato.
    }
}
