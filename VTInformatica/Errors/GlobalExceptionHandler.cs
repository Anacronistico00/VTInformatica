using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
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
    }
}
