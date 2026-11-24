namespace VTInformatica.Exceptions
{
    /// <summary>Lo stock del prodotto è cambiato rispetto a quello atteso da chi lo modifica.</summary>
    public class StockConflictException : DomainException
    {
        public int ProductId { get; }

        public StockConflictException(int productId)
            : base($"Lo stock del prodotto {productId} è cambiato nel frattempo: ricarica e riprova.", StatusCodes.Status409Conflict, "stock_conflict")
        {
            ProductId = productId;
        }
    }
}
