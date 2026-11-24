using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Account
{
    /// <summary>Corpo di PUT /Account/{id}/suspend: la nota per lo staff è obbligatoria e non viene mai mostrata al cliente.</summary>
    public class SuspendUserDto
    {
        // [Required] rifiuta anche stringhe vuote o di soli spazi.
        [Required]
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
