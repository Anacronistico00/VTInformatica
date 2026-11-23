namespace VTInformatica.Tests.Infrastructure;

/// <summary>
/// Tutti i test di integrazione condividono una sola factory (un host, un DB).
/// Ogni test crea dati propri (email unica, prodotto proprio), quindi non serve
/// resettare il DB tra un test e l'altro.
/// </summary>
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<VtWebApplicationFactory>
{
    public const string Name = "Api";
}
