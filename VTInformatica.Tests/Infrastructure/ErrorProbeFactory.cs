using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Exceptions;

namespace VTInformatica.Tests.Infrastructure;

/// <summary>
/// Factory con un controller di prova che lancia eccezioni a comando, per verificare il
/// gestore centralizzato degli errori. Il controller esiste solo qui: la factory condivisa
/// (VtWebApplicationFactory) non lo carica.
/// </summary>
public class ErrorProbeFactory : VtWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
            services.AddControllers().AddApplicationPart(typeof(ErrorProbeController).Assembly));
    }
}

[ApiController]
[Route("__test/errors")]
public class ErrorProbeController : ControllerBase
{
    // Testo che non deve mai uscire nel corpo della risposta.
    public const string SecretMarker = "SELECT * FROM SegretoInterno -- marker-7f3a";

    [HttpGet("unhandled")]
    public IActionResult Unhandled() => throw new InvalidOperationException(SecretMarker);

    [HttpGet("domain")]
    public IActionResult Domain() => throw new StockConflictException(42);

    [HttpPost("validation")]
    public IActionResult Validation([FromBody] ProbeDto dto) => Ok();

    public class ProbeDto
    {
        [Required]
        public string? Name { get; set; }
    }
}
