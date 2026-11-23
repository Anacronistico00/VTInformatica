using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using VTInformatica.Data;

namespace VTInformatica.Tests.Infrastructure;

/// <summary>
/// Avvia l'API reale in memoria con TestServer, sostituendo SQL Server con
/// SQLite in-memory e la configurazione JWT con valori di test.
/// </summary>
public class VtWebApplicationFactory : WebApplicationFactory<Program>
{
    // Chiave solo per i test (64 caratteri, sufficiente per HMAC-SHA256).
    // Non ha niente a che fare con la chiave di produzione.
    public const string JwtTestKey = "test-only-signing-key-0123456789-abcdefghijklmnopqrstuvwxyz-ABCD";
    public const string JwtIssuer = "https://tests.vtinformatica.local";
    public const string JwtAudience = "https://tests-audience.vtinformatica.local";

    // Unica origine CORS ammessa nei test.
    public const string AllowedCorsOrigin = "https://allowed-origin.test.local";

    // Il DB SQLite in-memory vive finché questa connessione resta aperta.
    private readonly SqliteConnection _connection;

    public VtWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // ApplicationUserRole.CreatedAt ha HasDefaultValueSql("GETDATE()"), che è
        // una funzione di SQL Server: la fornisco a SQLite per non toccare il modello.
        _connection.CreateFunction("GETDATE", () => DateTime.UtcNow);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Program.cs legge la sezione Jwt da builder.Configuration prima di Build():
        // UseSetting rende i valori disponibili già in quella fase.
        builder.UseSetting("Jwt:SecurityKey", JwtTestKey);
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);
        builder.UseSetting("Jwt:ExpiresInMinutes", "60");

        // Sovrascrive l'elemento 0 di Cors:AllowedOrigins di appsettings.json:
        // nei test l'unica origine ammessa è AllowedCorsOrigin.
        builder.UseSetting("Cors:AllowedOrigins:0", AllowedCorsOrigin);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
                ConfigureDbContext(options);
            });
        });
    }

    /// <summary>
    /// Punto di estensione per factory derivate (es. interceptor di fault injection).
    /// La factory condivisa non aggiunge nulla.
    /// </summary>
    protected virtual void ConfigureDbContext(DbContextOptionsBuilder options)
    {
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Le migrazioni sono specifiche di SQL Server: lo schema viene creato dal
        // modello (include gli HasData: ruoli, categorie, sottocategorie, produttori).
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
