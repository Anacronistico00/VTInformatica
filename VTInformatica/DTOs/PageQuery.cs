using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs
{
    /// <summary>
    /// Paginazione opzionale (query string). Senza <c>page</c> né <c>pageSize</c> la lista resta completa.
    /// Con uno dei due: pagina 1 e 20 elementi per i valori mancanti, al massimo 100 per pagina.
    /// </summary>
    public class PageQuery
    {
        public const string TotalCountHeader = "X-Total-Count";
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        [Range(1, int.MaxValue)]
        public int? Page { get; set; }

        [Range(1, MaxPageSize)]
        public int? PageSize { get; set; }

        public bool IsRequested => Page.HasValue || PageSize.HasValue;

        public int Take => PageSize ?? DefaultPageSize;

        public int Skip => (int)Math.Min((long)((Page ?? 1) - 1) * Take, int.MaxValue);
    }
}
