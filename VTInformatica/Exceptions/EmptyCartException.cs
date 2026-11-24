namespace VTInformatica.Exceptions
{
    /// <summary>Checkout con carrello vuoto: la richiesta è valida, lo stato del carrello no (409).</summary>
    public class EmptyCartException : DomainException
    {
        public EmptyCartException()
            : base("Il carrello è vuoto.", StatusCodes.Status409Conflict, "cart_empty")
        {
        }
    }
}
