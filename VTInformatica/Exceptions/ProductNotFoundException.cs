namespace VTInformatica.Exceptions
{
    /// <summary>Il prodotto richiesto non esiste.</summary>
    public class ProductNotFoundException : Exception
    {
        public int ProductId { get; }

        public ProductNotFoundException(int productId)
            : base($"Prodotto {productId} non trovato.")
        {
            ProductId = productId;
        }
    }
}
