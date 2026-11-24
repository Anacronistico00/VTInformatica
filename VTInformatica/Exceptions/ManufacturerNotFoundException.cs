namespace VTInformatica.Exceptions
{
    /// <summary>Il corpo della richiesta fa riferimento a un produttore inesistente (400).</summary>
    public class ManufacturerNotFoundException : DomainException
    {
        public ManufacturerNotFoundException(int manufacturerId)
            : base($"Il produttore {manufacturerId} non esiste.", StatusCodes.Status400BadRequest, "manufacturer_not_found")
        {
        }
    }
}
