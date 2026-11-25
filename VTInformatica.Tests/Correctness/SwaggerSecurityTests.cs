using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Correctness;

/// <summary>Swagger documenta lo schema Bearer JWT e lo richiede su tutta l'API.</summary>
[Collection(ApiCollection.Name)]
public class SwaggerSecurityTests
{
    private readonly VtWebApplicationFactory _factory;

    public SwaggerSecurityTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public void Swagger_DeclaresBearerJwtScheme()
    {
        var document = _factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

        var scheme = Assert.IsType<OpenApiSecurityScheme>(document.Components!.SecuritySchemes!["Bearer"]);
        Assert.Equal(SecuritySchemeType.Http, scheme.Type);
        Assert.Equal("Bearer", scheme.Scheme);
        Assert.Equal("JWT", scheme.BearerFormat);
        Assert.Equal(ParameterLocation.Header, scheme.In);
    }

    [Fact]
    public void Swagger_RequiresBearerOnTheApi()
    {
        var document = _factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

        Assert.Contains(document.Security!, requirement => requirement.Keys.Any(scheme => scheme.Reference?.Id == "Bearer"));
    }
}
