namespace VTInformatica.Exceptions
{
    /// <summary>La quantità richiesta supera lo stock disponibile del prodotto.</summary>
    public class InsufficientStockException : Exception
    {
        public int ProductId { get; }

        public InsufficientStockException(int productId)
            : base($"Prodotto {productId} non disponibile in quantità sufficiente.")
        {
            ProductId = productId;
        }
    }
}
