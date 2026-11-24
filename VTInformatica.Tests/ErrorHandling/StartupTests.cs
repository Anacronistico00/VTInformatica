using System.Diagnostics;

namespace VTInformatica.Tests.ErrorHandling;

/// <summary>
/// Un avvio fallito deve terminare con exit code diverso da 0 (prima il catch esterno di
/// Program.cs loggava solo ex.Message e il processo usciva con 0: un orchestratore o la CI
/// lo vedevano come successo).
/// Avvia l'API reale in un processo separato, quindi è più lento degli altri test.
/// </summary>
public class StartupTests
{
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task Startup_MissingJwtKey_ExitsWithNonZeroCode()
    {
        // VTInformatica.dll è copiato nella cartella di output dei test (ProjectReference).
        var appDll = typeof(Program).Assembly.Location;
        var workDir = Directory.CreateTempSubdirectory("vt-startup-test-").FullName;

        var startInfo = new ProcessStartInfo("dotnet", $"\"{appDll}\"")
        {
            // Cartella temporanea: i log su file di Serilog non finiscono nel repo.
            WorkingDirectory = workDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        // Production: niente User Secrets. Chiave JWT vuota: Program.cs deve fallire all'avvio.
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Production";
        startInfo.Environment["Jwt__SecurityKey"] = "";

        using var process = Process.Start(startInfo)!;
        // Svuota gli stream per non bloccare il processo su buffer pieni.
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();

        using var timeout = new CancellationTokenSource(ProcessTimeout);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"L'API non è terminata entro {ProcessTimeout.TotalSeconds}s: l'avvio non è fallito come atteso.");
        }
        finally
        {
            try { Directory.Delete(workDir, recursive: true); } catch (IOException) { }
        }

        await Task.WhenAll(stdout, stderr);
        Assert.NotEqual(0, process.ExitCode);
    }
}
