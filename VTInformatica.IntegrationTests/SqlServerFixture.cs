using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using VTInformatica.Data;

namespace VTInformatica.IntegrationTests;

/// <summary>
/// Un SQL Server vero per tutti i test di integrazione: un container mssql 2022, oppure il server
/// indicato da INTEGRATION_SQLSERVER (connection string verso il database master). Ogni test crea un
/// proprio database vuoto e ci applica le migration vere (non EnsureCreated).
/// Se non c'è né Docker né la variabile, la fixture fallisce con un messaggio chiaro: i test non vengono mai saltati.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    public const string ExternalServerVariable = "INTEGRATION_SQLSERVER";
    private const string Image = "mcr.microsoft.com/mssql/server:2022-latest";

    private readonly List<string> _databases = new();
    private MsSqlContainer? _container;
    private string _masterConnection = "";

    public async Task InitializeAsync()
    {
        var external = Environment.GetEnvironmentVariable(ExternalServerVariable);
        if (!string.IsNullOrWhiteSpace(external))
        {
            _masterConnection = external;
            return;
        }

        try
        {
            _container = new MsSqlBuilder(Image).Build();
            await _container.StartAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Test di integrazione: impossibile avviare SQL Server in un container (Docker assente o non in esecuzione). " +
                $"Avvia Docker, oppure imposta {ExternalServerVariable} con la connection string di un SQL Server (database master) da usare al posto del container.",
                ex);
        }

        _masterConnection = _container.GetConnectionString();
    }

    /// <summary>Crea un database vuoto e restituisce la connection string.</summary>
    public async Task<string> CreateDatabaseAsync()
    {
        var name = $"vt_it_{Guid.NewGuid():N}";
        await using (var connection = new SqlConnection(_masterConnection))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [{name}]";
            await command.ExecuteNonQueryAsync();
        }

        _databases.Add(name);
        return new SqlConnectionStringBuilder(_masterConnection) { InitialCatalog = name, Pooling = false }.ConnectionString;
    }

    /// <summary>Crea un database e ci applica tutte le migration.</summary>
    public async Task<string> CreateMigratedDatabaseAsync()
    {
        var connectionString = await CreateDatabaseAsync();
        await using var db = NewContext(connectionString);
        await db.Database.MigrateAsync();
        return connectionString;
    }

    public static ApplicationDbContext NewContext(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options);

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
            return;
        }

        // Server esterno: si eliminano solo i database creati da questa esecuzione.
        foreach (var name in _databases)
        {
            await using var connection = new SqlConnection(_masterConnection);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}];";
            await command.ExecuteNonQueryAsync();
        }
    }
}

[CollectionDefinition(Name)]
public class IntegrationCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
