using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VTInformatica.Exceptions;

namespace VTInformatica.Errors
{
    /// <summary>
    /// Gestore unico delle eccezioni (AUDIT #11), al posto dei try/catch nei controller.
    /// - DomainException: status e code dell'eccezione, messaggio come "detail". Log Warning.
    /// - Qualsiasi altra eccezione: 500 internal_error senza "detail": nessuno stack, messaggio SQL
    ///   o testo dell'eccezione esce verso il client, in nessun ambiente. Log Error con lo stack.
    /// È l'unico punto che logga le eccezioni: i controller non le catturano più.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
        {
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            int statusCode;
            string code;
            string? detail = null;

            if (exception is DomainException domain)
            {
                statusCode = domain.StatusCode;
                code = domain.Code;
                detail = domain.Message;
                Log.Warning("{Method} {Path} → {StatusCode} {Code}: {Message}",
                    httpContext.Request.Method, httpContext.Request.Path, statusCode, code, domain.Message);
            }
            else if (IsForeignKeyViolationOnDelete(exception))
            {
                // Rete di sicurezza: i service verificano già le dipendenze prima del delete; qui arriva
                // solo se un collegamento nasce tra la verifica e la delete (race). Non testabile su SQLite.
                statusCode = StatusCodes.Status409Conflict;
                code = "resource_in_use";
                detail = "La risorsa ha elementi collegati e non può essere eliminata.";
                Log.Warning("{Method} {Path} → 409 resource_in_use (vincolo FK in delete)",
                    httpContext.Request.Method, httpContext.Request.Path);
            }
            else
            {
                statusCode = StatusCodes.Status500InternalServerError;
                code = "internal_error";
                Log.Error(exception, "Eccezione non gestita su {Method} {Path} (traceId {TraceId})",
                    httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);
            }

            httpContext.Response.StatusCode = statusCode;
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = ApiProblems.Create(httpContext, statusCode, code, detail),
                Exception = exception,
            });
        }

        /// <summary>
        /// Violazione di FK durante una delete, riconosciuta dal codice numerico di SQL Server (547),
        /// non dal testo del messaggio. Il 547 vale anche per insert/update con riferimenti inesistenti:
        /// quei casi non sono "risorsa in uso" e restano 500 (vanno validati nei service, 400).
        /// </summary>
        private static bool IsForeignKeyViolationOnDelete(Exception exception) =>
            exception is DbUpdateException { InnerException: SqlException { Number: 547 } } dbUpdate
            && dbUpdate.Entries.Count > 0
            && dbUpdate.Entries.All(e => e.State == EntityState.Deleted);
    }
}
