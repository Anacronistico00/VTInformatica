using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VTInformatica.Errors
{
    /// <summary>
    /// Unico formato degli errori dell'API: ProblemDetails con estensione "code" (stabile, per il FE)
    /// e "traceId" (per ritrovare la richiesta nei log). Usato dal gestore globale, dai controller
    /// e dalla validazione del modello.
    /// </summary>
    public static class ApiProblems
    {
        public const string CodeKey = "code";
        public const string TraceIdKey = "traceId";

        /// <summary>Risposta di errore da un controller: <c>return this.ApiProblem(404, "not_found", "...")</c>.</summary>
        public static ObjectResult ApiProblem(this ControllerBase controller, int statusCode, string code, string? detail = null, string? title = null)
        {
            var problem = Create(controller.HttpContext, statusCode, code, detail, title);
            return new ObjectResult(problem) { StatusCode = statusCode };
        }

        public static ProblemDetails Create(HttpContext httpContext, int statusCode, string code, string? detail = null, string? title = null)
        {
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title ?? DefaultTitle(statusCode),
                Detail = detail,
            };
            problem.Extensions[CodeKey] = code;
            problem.Extensions[TraceIdKey] = TraceId(httpContext);
            return problem;
        }

        /// <summary>
        /// Applicato a ogni ProblemDetails prodotto dal framework (validazione del modello,
        /// NotFound() senza corpo, ecc.): aggiunge "code" e "traceId" se mancano.
        /// </summary>
        public static void Customize(ProblemDetailsContext context)
        {
            var problem = context.ProblemDetails;
            var status = problem.Status ?? context.HttpContext.Response.StatusCode;

            if (!problem.Extensions.ContainsKey(CodeKey))
            {
                problem.Extensions[CodeKey] = problem is ValidationProblemDetails ? "validation_failed" : DefaultCode(status);
            }
            if (!problem.Extensions.ContainsKey(TraceIdKey))
            {
                problem.Extensions[TraceIdKey] = TraceId(context.HttpContext);
            }
        }

        private static string TraceId(HttpContext httpContext) =>
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        private static string DefaultCode(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "bad_request",
            StatusCodes.Status401Unauthorized => "unauthorized",
            StatusCodes.Status403Forbidden => "forbidden",
            StatusCodes.Status404NotFound => "not_found",
            StatusCodes.Status409Conflict => "conflict",
            _ when statusCode >= 500 => "internal_error",
            _ => "error",
        };

        private static string DefaultTitle(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Richiesta non valida.",
            StatusCodes.Status401Unauthorized => "Non autenticato.",
            StatusCodes.Status403Forbidden => "Operazione non consentita.",
            StatusCodes.Status404NotFound => "Risorsa non trovata.",
            StatusCodes.Status409Conflict => "Conflitto con lo stato attuale della risorsa.",
            _ when statusCode >= 500 => "Errore interno del server.",
            _ => "Errore.",
        };
    }
}
