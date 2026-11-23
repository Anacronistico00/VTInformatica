using System.Security.Claims;

namespace VTInformatica.Extensions
{
    /// <summary>
    /// Identità dell'utente autenticato, letta sempre dai claim del JWT (mai da route o body).
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetEmail(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.Email);

        public static string? GetUserId(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        public static bool IsStaff(this ClaimsPrincipal user) =>
            user.IsInRole("Admin") || user.IsInRole("Seller");

        // Email del token uguale (case-insensitive) a quella indicata; false se il token non ha email.
        public static bool HasEmail(this ClaimsPrincipal user, string? email)
        {
            var current = user.GetEmail();
            return current != null && string.Equals(email, current, StringComparison.OrdinalIgnoreCase);
        }
    }
}
