# VTInformatica – audit "PRIMA" (baseline per il confronto prima/dopo)

Letto: BE (`VTInformatica`, ~2400 righe tra controller e service) e FE
(`vtinformatica_fe`, ~3100 righe di componenti). Ogni punto va riverificato
dall'agente in Plan mode prima di intervenire.

## Cosa è già buono (da non rompere)

- BE già a strati: Controllers → Services (con interfacce) → DbContext, DTO
  separati dalle entity, migrazioni EF, Serilog, Swagger con Bearer, soft delete
  degli ordini, password policy Identity.
- FE organizzato: Redux Toolkit con store persistito e scadenza, thunk per
  risorsa, rotte admin protette, toast.

## BE – problemi (per gravità)

### Critici (sicurezza / correttezza)

1. **Ogni registrazione crea un Admin**: `AccountController.Register` fa
   `AddToRoleAsync(newUser, "Admin")`. Chiunque si registra ha accesso a tutto
   il backoffice. Il ruolo di default deve essere `Customer`.
2. **IDOR su ordini e carrello** (non visibile dalla UI, il FE manda sempre i
   dati dell'utente loggato; sfruttabile solo chiamando l'API direttamente con
   un token valido): `GET /Order/user/{email}`,
   `POST /Order/from-cart/{userEmail}`, `/Cart/{email}/*` hanno solo
   `[Authorize]` e si fidano dell'email in route. L'email va letta dai claim del
   JWT.
3. **Cancella/ripristina ordine senza controllo di proprietà** (stesso discorso:
   dalla UI ognuno agisce solo sui propri ordini, ma l'API non lo impone):
   `PUT /Order/{id}/delete` e `/restore` sono `[Authorize]` senza controllo di
   proprietà; gli id sono interi sequenziali, quindi enumerabili (e il delete
   rimette a magazzino lo stock).
4. **Segreti committati**: chiave JWT in `appsettings.json`. Va ruotata e
   spostata in User Secrets / variabili d'ambiente (e il repo è pubblico, quindi
   la chiave attuale è da considerare compromessa).
5. **Nessun prezzo storico sull'ordine**: `OrderItem` non salva il prezzo; se
   cambia il prezzo del prodotto cambiano retroattivamente gli ordini vecchi e i
   totali.

### Importanti

4. **`PUT /Account/UpdateUser/{id}` senza `[Authorize]`** (severità media: il FE
   non lo chiama e l'id è un GUID non esposto da nessuna API, quindi sfruttarlo
   è difficile): resta un endpoint di scrittura aperto; deve richiedere login e
   usare l'id dai claim.
5. **Stock non atomico**: `CreateOrderFromCartAsync` controlla e scala lo stock
   senza transazione né concurrency token; due ordini simultanei possono vendere
   oltre la disponibilità. Due `SaveChanges` separati (ordine, poi svuotamento
   carrello) senza transazione.
6. **Validazione quantità**: `CartItemDto.Quantity` senza `[Range]`;
   `AddItemAsync` accetta quantità negative/zero e non controlla lo stock
   disponibile.
7. **`Register`**: `IsActive` arriva dal client; un utente disattivato può
   essere "riattivato" da chiunque registrandosi con la sua email.
8. **CORS `AllowAnyOrigin`** in tutti gli ambienti.
9. **Gestione errori**: ogni action ha lo stesso `try/catch` con 500 generico
   (boilerplate); manca un middleware centrale / `ProblemDetails`. In
   `Program.cs` il `catch` esterno logga solo `ex.Message` e inghiotte gli
   errori di avvio.
10. **Query**: `GetAllAsync` ordini e `GET /Product` senza paginazione;
    `GetOrdersByEmailAsync` include 5 livelli di navigazione per poi mappare
    solo nome/prezzo/immagini; nessun `AsNoTracking`. Possibile
    N+1/over-fetching.
11. **Mapping duplicato a mano**: lo stesso blocco entity→DTO è copiato più
    volte in `ProductController` e nei service (README cita AutoMapper ma non è
    referenziato nel csproj).
12. **Test: zero**. Nessun progetto test, nessuna CI.

### Minori

15. `DateTime.Now` per la scadenza del token (usare UTC); titolo Swagger
    "CustomersManager API"; `AccountService` senza interfaccia; messaggi misti
    IT/EN; `Log/` e file generati da verificare nel `.gitignore`.

## FE – problemi

1. **Token JWT in `localStorage`** (esposto a XSS) più ruolo e data di nascita
   duplicati in localStorage; il ruolo letto dal FE decide solo la UI (ok), ma
   non deve mai essere considerato sicurezza.
2. **URL backend hardcoded** in `Utils/ApiURL.js` (`https://localhost:7286`):
   niente env per dev/prod.
3. **Boilerplate `fetch` ripetuto** in tutte le actions (header, token da
   localStorage, gestione errori): manca un client API unico con gestione 401
   (logout/redirect) e refresh.
4. **L'email utente viaggia nell'URL** (`/Cart/{email}`, `/Order/user/{email}`):
   segue il problema IDOR lato BE.
5. **Nessun test**, nessuno script `test` in `package.json`, nessuna CI.
6. Componenti grandi (es. `ManageProductsComponent` 361 righe, `ProductInfo`
   313): logica e UI mescolate.
7. Admin CRUD senza validazione/conferme coerenti (da verificare nel dettaglio).

## Metriche baseline da registrare prima di iniziare

- Test BE/FE: 0 / 0. Warning `dotnet build`: <contare>. Problemi `npm run lint`:
  <contare>.
- Endpoint senza `[Authorize]` che modificano dati: 1 (`UpdateUser`). Endpoint
  con IDOR: ~8.

## Milestone proposte (un branch + PR ciascuna)

1. **M0 – Rete di sicurezza**: progetto xUnit + test di caratterizzazione su
   Cart/Order/Account (con SQLite in-memory o `WebApplicationFactory`); CI
   GitHub Actions (build + test BE, lint + build FE).
2. **M1 – Sicurezza BE**: ruolo `Customer` di default, email dai claim,
   controllo proprietà su ordini/carrello, `[Authorize]` su `UpdateUser`,
   rotazione chiave JWT + User Secrets, CORS ristretto. Un test per ogni falla
   chiusa (prima il test che fallisce, poi il fix).
3. **M2 – Ordini corretti**: prezzo su `OrderItem`, transazione + concurrency
   token sullo stock, validazione quantità.
4. **M3 – Refactor BE**: middleware errori + `ProblemDetails`, mapping
   centralizzato, paginazione e `AsNoTracking`.
5. **M4 – FE**: `apiClient` unico con 401 handling, `VITE_API_URL`, rimozione
   email dall'URL, split dei componenti grandi, test Vitest sui reducer e sui
   flussi carrello/ordine.
6. **M5 – Rifinitura**: README prima/dopo con le metriche, sezione "Come ho
   lavorato con l'AI", subagent reviewer + review automatica sulle PR.

---

## Stato avanzamento (aggiornato 2026-11-23 7:44PM)

Il testo sopra è la baseline e non va modificato.

| #                         | Stato             | Come                                                                                |
| ------------------------- | ----------------- | ----------------------------------------------------------------------------------- |
| 1 Ruolo Admin a tutti     | Chiuso            | M1a, test-first                                                                     |
| 2, 3 IDOR ordini/carrello | Chiuso            | M1b, M1c                                                                            |
| 4 UpdateUser senza auth   | Chiuso            | M1e                                                                                 |
| 5 Chiave JWT nel repo     | Chiuso nel codice | M1d. La chiave vecchia resta nella cronologia git: la neutralizza solo la rotazione |
| 10 CORS aperto            | Chiuso            | M1d                                                                                 |
| 14 Test e CI              | Chiuso            | M0: 0 → 42 test, CI su GitHub Actions                                               |
| 6, 7, 8                   | Aperti            | M2                                                                                  |
| 9, 11, 12, 13, 15         | Aperti            | M3 (15: DateTime.Now ancora presente)                                               |

### Nuovi finding emersi durante il lavoro

- Review: PUT e DELETE non hanno mai funzionato per nessuno (il token non aveva
  NameIdentifier). Chiuso in M1e, insieme all'identità presa dal body.
- Review: POST con ProductId inesistente risponde 500 (FK); ReviewRating non
  validato; nessuna moderazione admin.
- Review: da verificare se CustomerEmail esce dagli endpoint prodotto (possibile
  esposizione di email).
- UpdateUser: cambia Email senza aggiornare UserName né verificarne l'unicità;
  rompe il legame con carrello e ordini (chiave = email); DTO senza validazione.
- Carrello e ordini usano l'email come chiave: modificabile e non unica
  (RequireUniqueEmail non attivo). Valutare migrazione all'id.
- Avvio fallito: il catch esterno in Program.cs termina con exit code 0.
- Token ancora validi per utenti disattivati.
- Nome della macchina nella connection string di appsettings.json.
- Duplicazioni misurate: 39 try/catch con 500 generico in 7 controller; 18
  `new ...Dto` in ProductController; GetOrderDto ×3, GetReviewDto ×3; totale
  carrello ×2.
- Warning di build: 112.
- FE: `npm audit` 16 vulnerabilità (2 low, 2 moderate, 12 high), in gran parte
  dev-dependency; react-router a runtime. Da verificare: crash di NavBar dopo il
  reset della persistenza.
