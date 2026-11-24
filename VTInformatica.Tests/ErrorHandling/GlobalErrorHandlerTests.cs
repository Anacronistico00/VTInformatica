using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>
/// AUDIT #11: un gestore unico trasforma le eccezioni in ProblemDetails con "code" stabile
/// e traceId, senza dettagli interni (stack, messaggi SQL, testo dell'eccezione) in nessun ambiente.
/// </summary>
public class GlobalErrorHandlerTests : IClassFixture<ErrorProbeFactory>
{
    private readonly ErrorProbeFactory _factory;

    public GlobalErrorHandlerTests(ErrorProbeFactory factory) => _factory = factory;

    [Theory]
    [InlineData("Testing")]       // lo stesso ambiente di tutti gli altri test
    [InlineData("Development")]   // nessuna developer exception page al posto del gestore
    [InlineData("Production")]
    public async Task UnhandledException_Returns500ProblemDetails_WithoutInternalDetails(string environment)
    {
        await using var factory = environment == "Testing"
            ? null
            : _factory.WithWebHostBuilder(b => b.UseEnvironment(environment));
        var client = (factory ?? _factory).CreateClient();

        var response = await client.GetAsync("/__test/errors/unhandled");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SegretoInterno", text);
        Assert.DoesNotContain("marker-7f3a", text);
        Assert.DoesNotContain(nameof(InvalidOperationException), text);
        Assert.DoesNotContain(nameof(ErrorProbeController), text); // nessuna riga di stack

        var json = JsonDocument.Parse(text).RootElement;
        Assert.Equal(500, json.GetProperty("status").GetInt32());
        Assert.Equal("internal_error", json.GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(json.GetProperty("traceId").GetString()));
        Assert.False(json.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task DomainException_IsMappedToItsStatusAndCode()
    {
        var response = await _factory.CreateClient().GetAsync("/__test/errors/domain");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("stock_conflict", json.GetProperty("code").GetString());
        Assert.Contains("42", json.GetProperty("detail").GetString()); // messaggio di dominio, scritto da noi
        Assert.False(string.IsNullOrEmpty(json.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task ValidationError_HasErrorsAndValidationFailedCode()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/__test/errors/validation", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation_failed", json.GetProperty("code").GetString());
        Assert.True(json.GetProperty("errors").TryGetProperty("Name", out _));
        Assert.False(string.IsNullOrEmpty(json.GetProperty("traceId").GetString()));
    }
}
