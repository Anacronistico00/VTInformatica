using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Models;

namespace VTInformatica.IntegrationTests;

/// <summary>Le migration vere su SQL Server (Up e Down su un database vuoto) e i vincoli che creano.</summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public class MigrationAndConstraintTests
{
    private readonly SqlServerFixture _sql;

    public MigrationAndConstraintTests(SqlServerFixture sql) => _sql = sql;

    [Fact]
    public async Task AllMigrations_GoDownAndUpAgain_OnAnEmptyDatabase()
    {
        var connectionString = await _sql.CreateDatabaseAsync();
        await using var db = SqlServerFixture.NewContext(connectionString);
        var all = db.GetService<IMigrationsAssembly>().Migrations.Keys.ToList();
        Assert.NotEmpty(all);

        await db.Database.MigrateAsync();
        Assert.Equal(all, (await db.Database.GetAppliedMigrationsAsync()).ToList());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        var rolesAfterUp = await db.Roles.CountAsync();
        Assert.True(rolesAfterUp > 0, "Le migration devono seminare i ruoli.");

        // Down di tutte le migration, fino al database senza schema.
        await db.GetService<IMigrator>().MigrateAsync("0");
        Assert.Empty(await db.Database.GetAppliedMigrationsAsync());

        await db.Database.MigrateAsync();
        Assert.Equal(all, (await db.Database.GetAppliedMigrationsAsync()).ToList());
        Assert.Equal(rolesAfterUp, await db.Roles.CountAsync());
    }

    [Fact]
    public async Task OrderNumber_IsUnique()
    {
        await using var api = await ApiFactory.CreateAsync(_sql);
        var email = ApiFactory.UniqueEmail();
        await api.RegisterAndLoginAsync(email);

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userId = (await db.Users.AsNoTracking().SingleAsync(u => u.Email == email)).Id;
        var number = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        db.Orders.Add(new Order { OrderNumber = number, CustomerEmail = email, CustomerId = userId });
        await db.SaveChangesAsync();

        db.Orders.Add(new Order { OrderNumber = number, CustomerEmail = email, CustomerId = userId });
        var failure = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        // 2601 = indice unique violato (2627 se fosse un vincolo UNIQUE).
        var sqlError = Assert.IsType<SqlException>(failure.InnerException);
        Assert.Contains(sqlError.Number, new[] { 2601, 2627 });
    }
}
