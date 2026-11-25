using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Data;
using VTInformatica.Models;

namespace VTInformatica.IntegrationTests;

/// <summary>
/// Il 409 resource_in_use ha due strade: la verifica esplicita nel service e, se un collegamento nasce
/// tra la verifica e la DELETE, la rete di sicurezza sull'errore 547 di SQL Server.
/// Quest'ultima non è testabile su SQLite.
/// </summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public class ForeignKeySafetyNetTests
{
    private readonly SqlServerFixture _sql;

    public ForeignKeySafetyNetTests(SqlServerFixture sql) => _sql = sql;

    [Fact]
    public async Task Delete_CategoryWithSubCategory_Returns409ResourceInUse_ByServiceCheck()
    {
        await using var api = await ApiFactory.CreateAsync(_sql);
        var admin = api.ClientFor(await api.RegisterAndLoginAsync(ApiFactory.UniqueEmail(), admin: true));

        // La categoria 1 ha sottocategorie dal seed delle migration.
        var response = await admin.DeleteAsync("/Category/1");

        await AssertResourceInUseAsync(response);
    }

    [Fact]
    public async Task Delete_WhenALinkAppearsBetweenCheckAndDelete_Returns409ResourceInUse_ByFkSafetyNet()
    {
        var race = new InsertSubCategoryBeforeCategoryDelete();
        await using var api = await ApiFactory.CreateAsync(_sql, race);
        var admin = api.ClientFor(await api.RegisterAndLoginAsync(ApiFactory.UniqueEmail(), admin: true));
        var categoryId = await CreateEmptyCategoryAsync(api);
        race.Arm(api.ConnectionString, categoryId);

        // Il service vede la categoria senza sottocategorie; un'altra connessione ne inserisce una
        // un attimo prima della DELETE: SQL Server risponde 547 e il gestore lo traduce in 409.
        var response = await admin.DeleteAsync($"/Category/{categoryId}");

        Assert.True(race.Fired, "L'interceptor non è scattato: il test non ha simulato la corsa.");
        await AssertResourceInUseAsync(response);
        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.Categories.AnyAsync(c => c.Id == categoryId));
    }

    private static async Task AssertResourceInUseAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("resource_in_use", problem.GetProperty("code").GetString());
    }

    private static async Task<int> CreateEmptyCategoryAsync(ApiFactory api)
    {
        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = new Category { Name = $"Categoria corsa {Guid.NewGuid():N}" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category.Id;
    }

    /// <summary>Inserisce una sottocategoria, con un'altra connessione, subito prima della DELETE della categoria.</summary>
    private sealed class InsertSubCategoryBeforeCategoryDelete : DbCommandInterceptor
    {
        private string? _connectionString;
        private int _categoryId;

        public bool Fired { get; private set; }

        public void Arm(string connectionString, int categoryId)
        {
            _connectionString = connectionString;
            _categoryId = categoryId;
            Fired = false;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            InsertIfDeleteOfArmedCategory(command);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            InsertIfDeleteOfArmedCategory(command);
            return ValueTask.FromResult(result);
        }

        private void InsertIfDeleteOfArmedCategory(DbCommand command)
        {
            if (_connectionString is null || Fired || !command.CommandText.Contains("DELETE FROM [Categories]"))
                return;

            Fired = true;
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO SubCategories (Name, CategoryId) VALUES (N'Inserita nella corsa', @categoryId)";
            insert.Parameters.AddWithValue("@categoryId", _categoryId);
            insert.ExecuteNonQuery();
        }
    }
}
