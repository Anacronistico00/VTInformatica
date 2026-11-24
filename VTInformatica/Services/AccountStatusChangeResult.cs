namespace VTInformatica.Services
{
    /// <summary>Esito di suspend/unsuspend: il controller lo traduce nel codice HTTP.</summary>
    public enum AccountStatusChangeResult
    {
        Done,
        NotFound,
        CannotSuspendAdmin,
        NotSuspended
    }
}
