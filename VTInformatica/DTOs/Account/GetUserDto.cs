namespace VTInformatica.DTOs.Account
{
    /// <summary>Riga di GET /Account/UsersList (solo Admin): include i dati di disattivazione.</summary>
    public class GetUserDto
    {
        // Serve al backoffice per chiamare /Account/{id}/suspend e /unsuspend.
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateOnly BirthDate { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }

        // "User" / "Admin", null se l'account è attivo.
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public VTInformatica.Models.Auth.DeactivationReason? DeactivationReason { get; set; }

        public string? DeactivationNote { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }
}
