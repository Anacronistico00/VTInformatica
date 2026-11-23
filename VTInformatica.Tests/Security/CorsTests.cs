using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Security;

/// <summary>
/// CORS ristretto alle origini di Cors:AllowedOrigins (audit #10).
/// Si usa GET /Product perché è un endpoint pubblico.
/// </summary>
[Collection(ApiCollection.Name)]
public class CorsTests
{
    private const string AllowOriginHeader = "Access-Control-Allow-Origin";

    private readonly VtWebApplicationFactory _factory;

    public CorsTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task UnknownOrigin_NoAllowOriginHeader()
    {
        var response = await GetWithOriginAsync("https://evil.example");

        Assert.False(response.Headers.Contains(AllowOriginHeader));
    }

    [Fact]
    public async Task AllowedOrigin_EchoesOriginHeader()
    {
        var response = await GetWithOriginAsync(VtWebApplicationFactory.AllowedCorsOrigin);

        Assert.True(response.Headers.TryGetValues(AllowOriginHeader, out var values));
        Assert.Equal(VtWebApplicationFactory.AllowedCorsOrigin, Assert.Single(values!));
    }

    private Task<HttpResponseMessage> GetWithOriginAsync(string origin)
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/Product");
        request.Headers.Add("Origin", origin);
        return client.SendAsync(request);
    }
}
