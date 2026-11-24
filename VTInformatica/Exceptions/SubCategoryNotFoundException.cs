namespace VTInformatica.Exceptions
{
    /// <summary>Il corpo della richiesta fa riferimento a una sottocategoria inesistente (400).</summary>
    public class SubCategoryNotFoundException : DomainException
    {
        public SubCategoryNotFoundException(int subCategoryId)
            : base($"La sottocategoria {subCategoryId} non esiste.", StatusCodes.Status400BadRequest, "subcategory_not_found")
        {
        }
    }
}
