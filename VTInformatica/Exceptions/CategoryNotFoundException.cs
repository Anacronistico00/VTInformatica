namespace VTInformatica.Exceptions
{
    /// <summary>Il corpo della richiesta fa riferimento a una categoria inesistente (400).</summary>
    public class CategoryNotFoundException : DomainException
    {
        public CategoryNotFoundException(int categoryId)
            : base($"La categoria {categoryId} non esiste.", StatusCodes.Status400BadRequest, "category_not_found")
        {
        }
    }
}
