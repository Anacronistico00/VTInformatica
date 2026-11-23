using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VTInformatica.Models;

namespace VTInformatica.Tests.Infrastructure;

/// <summary>
/// Factory con un proprio DB SQLite in-memory e un interceptor che può far fallire
/// il salvataggio. Separata da quella condivisa, così gli altri test non ne risentono.
/// </summary>
public class FaultInjectionFactory : VtWebApplicationFactory
{
    public FailOnCartItemDeleteInterceptor CartItemDeleteFailure { get; } = new();

    protected override void ConfigureDbContext(DbContextOptionsBuilder options)
    {
        options.AddInterceptors(CartItemDeleteFailure);
    }
}

/// <summary>
/// Se attivo, fa fallire qualunque SaveChanges che cancella righe del carrello:
/// simula un errore durante lo svuotamento del carrello al checkout.
/// </summary>
public class FailOnCartItemDeleteInterceptor : SaveChangesInterceptor
{
    private volatile bool _enabled;

    public void Enable() => _enabled = true;
    public void Disable() => _enabled = false;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ThrowIfCartItemsDeleted(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ThrowIfCartItemsDeleted(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ThrowIfCartItemsDeleted(DbContext? context)
    {
        if (_enabled && context != null &&
            context.ChangeTracker.Entries<CartItem>().Any(e => e.State == EntityState.Deleted))
        {
            throw new InvalidOperationException("Fault injection: errore simulato durante lo svuotamento del carrello.");
        }
    }
}
