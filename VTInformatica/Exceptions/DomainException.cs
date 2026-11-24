namespace VTInformatica.Exceptions
{
    /// <summary>
    /// Errore di dominio previsto: il gestore globale lo traduce in ProblemDetails con
    /// <see cref="StatusCode"/>, <see cref="Code"/> stabile per il FE e il messaggio come "detail".
    /// Il messaggio viene mostrato al client: va scritto da noi, senza dati interni.
    /// </summary>
    public abstract class DomainException : Exception
    {
        public int StatusCode { get; }

        public string Code { get; }

        protected DomainException(string message, int statusCode, string code)
            : base(message)
        {
            StatusCode = statusCode;
            Code = code;
        }
    }
}
