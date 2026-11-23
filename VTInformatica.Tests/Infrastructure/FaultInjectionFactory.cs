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
    public ConcurrentStockWriterInterceptor ConcurrentStockWriter { get; } = new();

    protected override void ConfigureDbContext(DbContextOptionsBuilder options)
    {
        options.AddInterceptors(CartItemDeleteFailure, ConcurrentStockWriter);
    }
}

/// <summary>
/// Simula un ordine concorrente in modo deterministico: subito prima del primo
/// UPDATE su Products, porta lo stock del prodotto indicato a un nuovo valore,
/// come se un'altra richiesta avesse committato tra la lettura e la scrittura.
/// Scatta una sola volta per ogni Arm.
/// Con una sola connessione SQLite la scrittura simulata sta nella stessa transazione
/// della richiesta e un rollback la annullerebbe: per questo lo stock viene misurato
/// subito dopo il nostro UPDATE (StockAfterOurUpdate), prima di commit/rollback.
/// </summary>
public class ConcurrentStockWriterInterceptor : DbCommandInterceptor
{
    private readonly object _lock = new();
    private (int ProductId, int Stock)? _pending;
    private int? _measureProductId;

    /// <summary>Stock del prodotto subito dopo il nostro UPDATE (null se non misurato).</summary>
    public int? StockAfterOurUpdate { get; private set; }

    public void Arm(int productId, int stockWrittenByOtherOrder)
    {
        lock (_lock)
        {
            _pending = (productId, stockWrittenByOtherOrder);
            _measureProductId = null;
            StockAfterOurUpdate = null;
        }
    }

    public void Disarm()
    {
        lock (_lock)
        {
            _pending = null;
            _measureProductId = null;
        }
    }

    public override System.Data.Common.DbDataReader ReaderExecuted(
        System.Data.Common.DbCommand command, CommandExecutedEventData eventData, System.Data.Common.DbDataReader result)
    {
        MeasureStockIfPending(command);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(
        System.Data.Common.DbCommand command, CommandExecutedEventData eventData, System.Data.Common.DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        MeasureStockIfPending(command);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        System.Data.Common.DbCommand command, CommandExecutedEventData eventData, int result)
    {
        MeasureStockIfPending(command);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        System.Data.Common.DbCommand command, CommandExecutedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        MeasureStockIfPending(command);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void MeasureStockIfPending(System.Data.Common.DbCommand command)
    {
        int productId;
        lock (_lock)
        {
            if (_measureProductId == null || !command.CommandText.Contains("UPDATE \"Products\""))
                return;
            productId = _measureProductId.Value;
            _measureProductId = null;
        }

        using var read = command.Connection!.CreateCommand();
        read.Transaction = command.Transaction;
        read.CommandText = "SELECT \"Quantity\" FROM \"Products\" WHERE \"Id\" = $id";
        var id = read.CreateParameter();
        id.ParameterName = "$id";
        id.Value = productId;
        read.Parameters.Add(id);
        StockAfterOurUpdate = Convert.ToInt32(read.ExecuteScalar());
    }

    public override InterceptionResult<System.Data.Common.DbDataReader> ReaderExecuting(
        System.Data.Common.DbCommand command, CommandEventData eventData, InterceptionResult<System.Data.Common.DbDataReader> result)
    {
        WriteConcurrentStockIfArmed(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<System.Data.Common.DbDataReader>> ReaderExecutingAsync(
        System.Data.Common.DbCommand command, CommandEventData eventData, InterceptionResult<System.Data.Common.DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        WriteConcurrentStockIfArmed(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        System.Data.Common.DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        WriteConcurrentStockIfArmed(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        System.Data.Common.DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        WriteConcurrentStockIfArmed(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void WriteConcurrentStockIfArmed(System.Data.Common.DbCommand command)
    {
        (int ProductId, int Stock) pending;
        lock (_lock)
        {
            if (_pending == null || !command.CommandText.Contains("UPDATE \"Products\""))
                return;
            pending = _pending.Value;
            _pending = null;
            _measureProductId = pending.ProductId;
        }

        // Stessa connessione e stessa transazione del comando intercettato (SQLite in-memory condiviso).
        using var concurrentWrite = command.Connection!.CreateCommand();
        concurrentWrite.Transaction = command.Transaction;
        concurrentWrite.CommandText = "UPDATE \"Products\" SET \"Quantity\" = $stock WHERE \"Id\" = $id";
        var stock = concurrentWrite.CreateParameter();
        stock.ParameterName = "$stock";
        stock.Value = pending.Stock;
        concurrentWrite.Parameters.Add(stock);
        var id = concurrentWrite.CreateParameter();
        id.ParameterName = "$id";
        id.Value = pending.ProductId;
        concurrentWrite.Parameters.Add(id);
        concurrentWrite.ExecuteNonQuery();
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
