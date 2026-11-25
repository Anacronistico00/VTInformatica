# VTInformatica BE – API e-commerce prodotti tech per PC

## Stack

- ASP.NET Core Web API (net10.0), EF Core 10 (SQL Server), ASP.NET Identity + JWT
  Bearer, Serilog, Swagger
- xUnit + WebApplicationFactory

## Comandi

- Build: `dotnet build VTInformatica.sln`
- Run: `dotnet run --project VTInformatica` (https://localhost:7286, Swagger in
  Development)
- Test: `dotnet test`
- Migrazioni: `dotnet ef migrations add <Nome> --project VTInformatica` /
  `dotnet ef database update`
- DB iniziale: `DatabaseScript.sql` (ruoli Admin, Seller, Customer già inseriti)

## Struttura (`VTInformatica/`)

- `Controllers/`: un controller per risorsa, route `[controller]`
- `Services/` + `Interfaces/`: logica di business (Account, Cart, Category,
  Manufacturer, Order, Product, Review, SubCategory)
- `Models/` (entity EF, `Models/Auth` per Identity), `DTOs/<Risorsa>/`,
  `Data/ApplicationDbContext.cs`, `Migrations/`
- Config: `appsettings*.json`, sezione `Jwt` e `Identity`

## Convenzioni

- Controller sottili: validazione input, chiamata al service, mapping risposta.
  Mai query EF nei controller.
- Mai esporre le entity: sempre DTO. Il mapping manuale è duplicato in più
  punti: se lo tocchi, centralizzalo in un solo punto invece di aggiungere altre
  copie.
- Identità utente SEMPRE dai claim del JWT
  (`User.FindFirstValue(ClaimTypes.Email)`), mai da route/body.
- Autorizzazione per ruoli: `Admin`, `Seller`, `Customer`. Ogni endpoint non
  pubblico deve avere `[Authorize]` esplicito.
- Operazioni che toccano più tabelle (ordine + stock + carrello) vanno in
  un'unica transazione.
- Async/await ovunque. Date in UTC (`DateTime.UtcNow`).
- Log con Serilog (`Log.Error(ex, ...)`), mai swallow silenzioso delle
  eccezioni.
- I commenti spiegano il perché del codice attuale, mai la sua storia: niente
  "prima era...", "fix di...", riferimenti a milestone.

## Regole di lavoro

- Prima di modificare codice: proponi un piano e aspetta conferma.
- Un task = un branch = commit piccoli. Modifiche minime e mirate, niente
  riscritture non richieste.
- Ogni cambio di logica ha un test. Dopo ogni modifica: `dotnet build` e
  `dotnet test`.
- Non toccare migrazioni già applicate. Mai committare segreti (chiave JWT,
  connection string con credenziali): usare User Secrets / variabili d'ambiente.
- Se una scelta di architettura è ambigua, chiedi invece di decidere da solo.
- Bug noti: per ogni falla prima un test rosso, poi il fix minimo. I test in
  `Characterization/` con commento `AUDIT #n` documentano bug ancora aperti: non
  "sistemarli" senza istruzione esplicita, vanno invertiti solo nella fetta che
  chiude quel punto.
- DB nei test: SQLite in-memory. Non verifica le migrazioni SQL Server né la
  concorrenza (stock): quei test richiederanno SQL Server reale.

## Flusso Git

- Branch di lavoro sempre da `develop`, con nome `fix/...`, `test/...` o
  `chore/...`
- PR verso `develop`. `master` riceve solo il merge da `develop` a fine blocco
  di milestone
- Mai push o merge diretti su `master` o `develop`
- La CI (`build-and-test`) deve essere verde prima di ogni merge
- Non aggiungere mai Co-Authored-By, 'Generated with Claude Code' né link di
  sessione Claude a commit, PR o file.
