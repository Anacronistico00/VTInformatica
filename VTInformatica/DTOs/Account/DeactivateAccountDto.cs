using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Account
{
    /// <summary>Corpo facoltativo di DELETE /Account/{id}: motivo scritto dal cliente.</summary>
    public class DeactivateAccountDto
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
