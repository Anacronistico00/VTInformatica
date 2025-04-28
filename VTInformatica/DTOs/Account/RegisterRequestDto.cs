using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Account
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateOnly BirthDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
