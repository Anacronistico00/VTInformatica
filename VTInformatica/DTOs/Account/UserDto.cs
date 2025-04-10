using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Account
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public bool IsActive { get; set; }
    }
}
