namespace VTInformatica.Exceptions
{
    /// <summary>
    /// L'email non si cambia dalla modifica del profilo: carrello e ordini la usano come chiave.
    /// </summary>
    public class EmailChangeNotAllowedException : DomainException
    {
        public EmailChangeNotAllowedException()
            : base("L'email non può essere modificata da questo endpoint.", StatusCodes.Status400BadRequest, "email_change_not_allowed")
        {
        }
    }
}
