﻿using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Account
{
    public class GetUsersResponseDto
    {
        [Required]
        public required string message { get; set; }
        public ICollection<GetUserDto> users { get; set; }
    }
}
