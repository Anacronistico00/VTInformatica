using System.Net;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests;

/// <summary>
/// Verifica l'infrastruttura di test: host avviato su SQLite e JWT emesso/validato
/// con la chiave di test configurata dalla factory.
/// </summary>
[Collection(ApiCollection.Name)]
public class SmokeTests
{
    private readonly VtWebApplicationFactory _factory;

    public SmokeTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task AuthorizedEndpoint_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Cart/{TestHelpers.UniqueEmail()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthorizedEndpoint_WithTokenFromLogin_Returns200()
    {
        var email = TestHelpers.UniqueEmail();
        var token = await TestHelpers.RegisterAndLoginAsync(_factory, email);
        var client = TestHelpers.AuthorizedClient(_factory, token);

        var response = await client.GetAsync($"/Cart/{email}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
