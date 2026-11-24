namespace VTInformatica.DTOs.Account
{
    public class UpdateUserDto
    {
        // Corpo della richiesta: se manca il campo resta null e la validazione risponde 400.
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        // Opzionale e non modificabile: se presente deve coincidere con quella attuale (altrimenti 400).
        public string? Email { get; set; }
        public DateOnly BirthDate { get; set; }
    }
}
