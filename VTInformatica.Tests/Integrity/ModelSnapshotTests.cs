using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using VTInformatica.Data;
using VTInformatica.Models.Auth;

namespace VTInformatica.Tests.Integrity;

/// <summary>
/// Il modello EF deve coincidere con lo snapshot delle migrazioni (equivalente di
/// `dotnet ef migrations has-pending-model-changes`). Con il seed dei ruoli basato su
/// Guid.NewGuid() ogni build del modello produce dati diversi, e ogni nuova migration
/// cancellerebbe e reinserirebbe i ruoli svuotando AspNetUserRoles.
/// Resta anche come guardia: una modifica al modello senza migration fa fallire la CI.
/// </summary>
public class ModelSnapshotTests
{
    // GUID già presenti nei DB esistenti: migration 20250508102202_Update, snapshot,
    // DatabaseScript.sql e DB reale.
    private static readonly Dictionary<string, string> AppliedRoleIds = new()
    {
        ["Admin"] = "98b4e551-a53d-40fc-9fa4-29d6dd582481",
        ["Seller"] = "9ab947f2-b737-46bf-a388-2f8962573a2b",
        ["Customer"] = "bd7ef491-0f1b-4b8c-9c80-340e14cfac9b",
    };

    // Provider SQL Server come le migrazioni; la connection string non viene mai aperta.
    private static ApplicationDbContext CreateSqlServerContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options);

    [Fact]
    public void Model_HasNoPendingChanges_ComparedToSnapshot()
    {
        using var context = CreateSqlServerContext();

        IModel? snapshotModel = context.GetService<IMigrationsAssembly>().ModelSnapshot?.Model;
        Assert.NotNull(snapshotModel);

        // EF 8: il modello dello snapshot va finalizzato e inizializzato prima del confronto,
        // come fa il comando has-pending-model-changes.
        if (snapshotModel is IMutableModel mutableModel)
        {
            snapshotModel = mutableModel.FinalizeModel();
        }
        snapshotModel = context.GetService<IModelRuntimeInitializer>()
            .Initialize(snapshotModel, designTime: true, validationLogger: null);

        var designTimeModel = context.GetService<IDesignTimeModel>().Model;

        var operations = context.GetService<IMigrationsModelDiffer>()
            .GetDifferences(snapshotModel.GetRelationalModel(), designTimeModel.GetRelationalModel());

        Assert.True(operations.Count == 0,
            "Modifiche al modello senza migration:\n" + string.Join("\n", operations.Select(Describe)));
    }

    [Fact]
    public void RoleSeed_UsesIdsOfAppliedMigrations()
    {
        using var context = CreateSqlServerContext();

        var seededRoles = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(ApplicationRole))!
            .GetSeedData()
            .ToDictionary(r => (string)r[nameof(ApplicationRole.Name)]!, r => (string)r[nameof(ApplicationRole.Id)]!);

        Assert.Equal(AppliedRoleIds, seededRoles);
    }

    private static string Describe(MigrationOperation operation) => operation switch
    {
        InsertDataOperation insert => $"InsertData {insert.Table}",
        DeleteDataOperation delete => $"DeleteData {delete.Table}",
        UpdateDataOperation update => $"UpdateData {update.Table}",
        ITableMigrationOperation table => $"{operation.GetType().Name} {table.Table}",
        _ => operation.GetType().Name,
    };
}
