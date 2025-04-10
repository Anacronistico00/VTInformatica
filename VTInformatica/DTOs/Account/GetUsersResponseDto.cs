using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Account
{
    public class GetUsersResponseDto
    {
        [Required]
        public required string message { get; set; }
        public ICollection<UserDto> users { get; set; }
    }
}
