namespace VTInformatica.Exceptions
{
    /// <summary>
    /// La risorsa ha elementi collegati e non può essere eliminata (409). Comportamento voluto:
    /// contano anche i prodotti eliminati (soft delete), che restano nel DB per lo storico ordini.
    /// </summary>
    public class ResourceInUseException : DomainException
    {
        public ResourceInUseException(string message)
            : base(message, StatusCodes.Status409Conflict, "resource_in_use")
        {
        }
    }
}
