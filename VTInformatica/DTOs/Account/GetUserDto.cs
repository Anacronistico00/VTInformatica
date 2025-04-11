namespace VTInformatica.DTOs.Account
{
    public class GetUserDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateOnly BirthDate { get; set; }
        public bool IsActive { get; set; }
    }
}
