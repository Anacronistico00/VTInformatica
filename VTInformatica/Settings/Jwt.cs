namespace VTInformatica.Settings
{
    public class Jwt
    {
        public required string SecurityKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required double ExpiresInMinutes { get; set; }
    }
}
