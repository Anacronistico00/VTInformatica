namespace VTInformatica.Exceptions
{
    /// <summary>Il prodotto richiesto non esiste.</summary>
    public class ProductNotFoundException : DomainException
    {
        public int ProductId { get; }

        public ProductNotFoundException(int productId)
            : base($"Prodotto {productId} non trovato.", StatusCodes.Status404NotFound, "product_not_found")
        {
            ProductId = productId;
        }
    }
}
