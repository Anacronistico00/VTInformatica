namespace VTInformatica.Exceptions
{
    /// <summary>La quantità richiesta supera lo stock disponibile del prodotto.</summary>
    public class InsufficientStockException : DomainException
    {
        public int ProductId { get; }

        public InsufficientStockException(int productId)
            : base($"Prodotto {productId} non disponibile in quantità sufficiente.", StatusCodes.Status409Conflict, "insufficient_stock")
        {
            ProductId = productId;
        }
    }
}
