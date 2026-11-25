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

---

## Stato avanzamento M2 (aggiornato 2026-11-23)

Branch `fix/m2-orders-correctness`, test-first (per ogni punto prima il test
rosso, poi il fix). Le sezioni sopra non sono state modificate: questa aggiorna
la riga "6, 7, 8 – Aperti – M2" della tabella precedente.

| #                      | Stato  | Come                                                                                                                                             |
| ---------------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| 6 Prezzo storico       | Chiuso | `OrderItem.UnitPrice` salvato al checkout, gli ordini lo mostrano al posto del prezzo corrente. Migration `AddUnitPriceToOrderItem` con backfill |
| 7 Stock / transazione  | Chiuso | Checkout in un'unica transazione (tutto o niente); decremento atomico condizionale `UPDATE … WHERE Quantity >= qty`, in ordine di `ProductId`    |
| 8 Validazione quantità | Chiuso | `[Range(1, …)]` su `CartItemDto` → 400; stock insufficiente → 409 (carrello e checkout); prodotto inesistente → 404                              |

- Test: 42 → 54 (cartella `Correctness/`). Warning di build: 110.
- Mapping ordine → DTO centralizzato in `OrderService.ToDto` (era copiato 3
  volte).
- Nuove eccezioni di dominio generiche in `Exceptions/`
  (`ProductNotFoundException`, `InsufficientStockException`), catturate nei
  controller prima del catch generico.
- **FE deve gestire 400/404/409 su carrello e checkout (M4).**

### Limiti noti

- **Backfill approssimato**: per le righe d'ordine esistenti `UnitPrice` è il
  prezzo corrente del prodotto al momento della migration; il prezzo storico
  reale non era mai stato salvato.
- **Migration non verificata da CI**: i test usano SQLite con `EnsureCreated`.
  Migration e backfill vanno provati con `dotnet ef database update` su SQL
  Server prima del deploy.
- **Concorrenza simulata**: SQLite in-memory con una sola connessione non
  riproduce un race reale. L'ordine concorrente è simulato in modo
  deterministico da un interceptor (scrittura dello stock tra lettura e
  aggiornamento). Il race reale con N checkout paralleli richiede SQL Server
  (Testcontainers): previsto in un branch `test/...` dopo M2.
- Il controllo dello stock all'aggiunta al carrello è indicativo; la garanzia
  vera è al checkout.

### Fuori scope emersi in M2 (non toccati)

1. **`OrderItem → Product` in cascade delete**: cancellare un prodotto cancella
   lo storico ordini.
2. **Seed dei ruoli con `Guid.NewGuid()`** (`ApplicationDbContext`): ogni
   `migrations add` genera Delete/InsertData dei ruoli e svuota AspNetUserRoles:
   da correggere prima di qualsiasi altra migration o upgrade a EF Core 9. In M2
   le operazioni sui ruoli sono state rimosse a mano dalla migration, dallo
   snapshot e dal Designer.
3. `SoftDeleteAsync` / `RestoreAsync`: stesso read-modify-write sullo stock;
   `Restore` con stock insufficiente risponde 404 (fuorviante).
4. Modifica prodotto lato admin: può sovrascrivere `Quantity` con un valore
   vecchio (lost update).
5. `CreateProductDto.Quantity` / `UpdateProductDto.Quantity` senza `[Range]`: lo
   stock può diventare negativo.
6. DTO morti, non usati da controller o service: `CartDto`, `OrderItemDto`,
   `CreateOrderDto`, `OrderDto`.
7. `GetCartByEmailAsync` aggiunge un `Cart` al context dentro un getter;
   `ItemBelongsToCurrentUserAsync` carica l'intero carrello.
8. `Cart.TotalPrice` persistito, ridondante e ricalcolato due volte in
   `AddItemAsync`.
9. `CartController.AddItem`: controllo `userId` ridondante, la risposta non
   restituisce il carrello.
10. `OrderNumber` = primi 8 caratteri di un GUID, senza indice unique.
11. `CustomerComment` hardcoded ("Ordine creato dal carrello").
12. `Product.Price` senza precisione esplicita.
13. Carrello vuoto al checkout → 500 (#11, M3).
14. `GetOrdersByEmailAsync` con 5 livelli di include (#12, M3).

---

## M3 – mappa delle fette (2026-11-24)

M3 è diviso in fette mergiabili singolarmente: **M3a1** integrità prodotti,
**M3a2** integrità account (#9, UpdateUser email, token di utenti disattivati),
**M3b** gestione errori (#11), **M3c** regole residue ordini/review, **M3d**
pulizia e performance (#12, #13, #15). Restano fuori da M3: chiave email → id
utente (milestone dedicata dopo M4), moderazione delle review, race reale con
SQL Server (branch `test/...`).

## Stato avanzamento M3a1 (aggiornato 2026-11-24)

Branch `fix/m3a1-product-integrity`, test-first. Chiude i fuori scope M2 n. 1,
2, 4, 5.

| Voce                              | Stato          | Come                                                                                                                                                                                           |
| --------------------------------- | -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Seed ruoli con `Guid.NewGuid()`   | Chiuso         | GUID fissi, uguali a quelli dei DB esistenti (migration `20250508102202_Update`, `DatabaseScript.sql`, DB reale). Test che confronta modello e snapshot: blocca in CI le migration dimenticate |
| `OrderItem → Product` in cascade  | Chiuso         | Soft delete: `Product.IsDeleted` + query filter globale, FK `Restrict`. Gli ordini usano `IgnoreQueryFilters` e conservano le righe. Migration `SoftDeleteProducts`                            |
| Prodotto eliminato nei carrelli   | Chiuso         | Le righe vengono rimosse e `Cart.TotalPrice` ricalcolato nello stesso `SaveChanges` del delete                                                                                                 |
| Quantity/Price senza `[Range]`    | Chiuso         | Quantity ≥ 0, Price > 0 su Create/UpdateProductDto → 400                                                                                                                                       |
| Update admin sovrascrive lo stock | Chiuso lato BE | `ExpectedQuantity` opzionale: update condizionale atomico, 409 se lo stock è cambiato, tutto o niente                                                                                          |

- Test: 54 → 70 (cartella `Integrity/`). Warning di build: 110 (invariati).
- **FE deve inviare `ExpectedQuantity` nella modifica prodotto e gestire 409
  (M4).** Senza il campo, la PUT sovrascrive ancora lo stock come prima.

### Limiti noti

- **Migration non verificata da CI** (SQLite + `EnsureCreated`): lo script
  (`DROP/ADD CONSTRAINT ... ON DELETE NO ACTION`,
  `ADD IsDeleted bit NOT NULL DEFAULT 0`) va provato con
  `dotnet ef database update` su SQL Server prima del deploy.
- Le review di un prodotto eliminato restano visibili in `GET /Review`.
- Un Manufacturer o una SubCategory con soli prodotti eliminati non si può più
  cancellare (FK Restrict sulle righe soft-deleted): oggi risponde 500,
  diventerà 409 in M3b.

### Fuori scope emersi in M3a1 (non toccati)

1. `GET /Product/search` risponde 404 quando non trova nulla, invece di una
   lista vuota.
2. Ripristino di un prodotto eliminato: nessun endpoint.

## Stato avanzamento M3a2 (aggiornato 2026-11-24)

Branch `fix/m3a2-account-integrity`, test-first. Chiude #9 e due finding emersi
in M1.

| Voce                                | Stato  | Come                                                                                                                                       |
| ----------------------------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| 9 Register su email disattivata     | Chiuso | Stessa risposta 400 "Email già registrata" dell'email attiva: nessuna sovrascrittura del profilo, lo stato dell'account non viene rivelato |
| 9 `IsActive` dal client             | Chiuso | Rimosso da `RegisterRequestDto`, deciso dal server (campo eventualmente inviato dal FE ignorato)                                           |
| UpdateUser cambia Email             | Chiuso | Email opzionale e non modificabile: se diversa da quella attuale → 400. Carrello e ordini restano raggiungibili                            |
| Token validi per utenti disattivati | Chiuso | `JwtBearerEvents.OnTokenValidated`: utente inesistente o disattivato → 401, effetto immediato                                              |

- Test: 70 → 77. Warning di build: 110 → 109.
- I nomi mancanti in UpdateUser erano già rifiutati (stringhe non-nullable con
  `<Nullable>enable</Nullable>`): il finding "DTO senza validazione" valeva solo
  per l'email, ora opzionale.
- Il controllo del token fa una lettura dell'utente per ogni richiesta
  autenticata.
- **FE (M4)**: gestire 401 su qualsiasi chiamata autenticata (account
  disattivato) con logout; non inviare più `IsActive` nella registrazione.

### Prossima fetta: disattivazione, sospensione e riattivazione (`feat/account-reactivation`)

Solo documentata, da pianificare dopo M3a2. Oggi esiste solo la disattivazione
da Admin (`PUT /Account/DeleteUserByEmail/{email}`), senza motivo né modo di
tornare indietro. (Terminologia aggiornata: "ban" → "sospensione", vedi lo stato
avanzamento più sotto.)

- **Nuovi campi su `ApplicationUser`** (migration): `DeactivationReason`
  (User/Admin), `DeactivationNote` (testo, max 500; obbligatorio per la
  sospensione, facoltativo per la cancellazione del cliente), `DeactivatedAt`
  (UTC). Azzerati alla riattivazione.
- **`DELETE /Account/{id}`** del cliente su se stesso → Reason=User, nota
  facoltativa. DELETE di un admin su un altro utente: da decidere nel piano
  (proposta: rifiutato, usare `/suspend`).
- **Sospensione**: `PUT /Account/{id}/suspend {note}` e
  `PUT /Account/{id}/unsuspend`, solo Admin. Effetto immediato grazie al
  controllo del token di M3a2. Un admin non può sospendere se stesso.
- **Login con password corretta**: Reason=User → 403 `account_deactivated`;
  Reason=Admin → 403 `account_suspended` (messaggio generico, la nota non viene
  mostrata al cliente). Password errata o email inesistente → 401 generico.
- **`POST /Account/reactivate {email, password}`**: riverifica la password con
  lockout Identity; solo Reason=User, altrimenti 403 `account_suspended`.
  Riattiva e restituisce il token.
- **FE (M4)**: pagina di riattivazione con conferma e pagina di benvenuto;
  messaggio di negazione per account sospeso; campo motivo facoltativo nella
  cancellazione account; nota obbligatoria nella sospensione da admin.

Decisioni prese (2026-11-24):

1. **403 solo dopo la verifica della password.** Oggi `Login` risponde 401 a un
   utente inattivo prima di controllare la password; `account_deactivated` /
   `account_suspended` vanno restituiti solo con password corretta, altrimenti
   401 generico (nessuna enumerazione dello stato dell'account).
2. **`PUT /Account/DeleteUserByEmail/{email}` sostituito da
   `PUT /Account/{id}/suspend`.**
3. **Utenti già disattivati** al momento della migration →
   `DeactivationReason = Admin` (l'unico modo di disattivare esistente finora).

## Fuori scope per M3d emersi dallo smoke test di M3a1 (non toccati)

1. **`manufacturer.manufacturerId` sempre 0** nella risposta di `GET /Product`:
   il DTO annidato `ManufacturerDto.ManufacturerId` non viene mai valorizzato
   dal mapping (es. `ProductController.cs`), mentre `product.manufacturerId` è
   corretto. Rientra nella centralizzazione dei mapping (#13).
2. **Nome prodotto con spazi iniziali**: il prodotto 18 è
   `"    Intel Core i9-14900K (3,2 GHz / 5,8 GHz)"`. Il dato arriva da
   `DatabaseScript.sql:521`; in più Create/Update prodotto non fanno Trim su
   `Name` (e `Description`), quindi lo stesso problema può arrivare dall'API.
   Fix: Trim in create/update e correzione del dato esistente.
3. **Refuso "Perferiche"**: la categoria 2 è `N'Perferiche'` in
   `DatabaseScript.sql:422` e nei DB creati da lì; il seed (`HasData`) dice già
   "Periferiche". Correzione del dato con uno script o da admin, senza
   migration.

## Stato avanzamento feat/account-reactivation (aggiornato 2026-11-24)

Branch `feat/account-reactivation`, test-first. Implementa la specifica
"disattivazione, sospensione e riattivazione" con le decisioni sopra.

| Voce                          | Stato   | Come                                                                                                                                                                                                  |
| ----------------------------- | ------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Campi di disattivazione       | Fatto   | `DeactivationReason` (User=1/Admin=2), `DeactivationNote` (max 500), `DeactivatedAt` (UTC). Migration `AccountDeactivation` con backfill `Reason=Admin` per gli utenti già disattivati                |
| Login senza enumerazione      | Chiuso  | `CheckPasswordSignInAsync` prima dello stato: email inesistente, password errata e lockout → stesso 401 `invalid_credentials`. Con password corretta: 403 `account_deactivated` / `account_suspended` |
| Lockout                       | Fatto   | Attivo su login e reactivate (contatore unico): 5 tentativi, 15 minuti (`appsettings` → `Identity`)                                                                                                   |
| `DELETE /Account/{id}`        | Fatto   | Solo sul proprio account, nota facoltativa → 204. Altro utente → 403 (anche Admin: usare `/suspend`); Admin su se stesso → 403                                                                        |
| `PUT /Account/{id}/suspend`   | Fatto   | Solo Admin, nota obbligatoria. 400 `cannot_suspend_self`, 409 `cannot_suspend_admin`, 404. Un utente già disattivato diventa sospeso con la nuova nota                                                |
| `PUT /Account/{id}/unsuspend` | Fatto   | Solo Admin: riattiva e azzera i campi. 409 `not_suspended` per account attivi o disattivati dal cliente                                                                                               |
| `POST /Account/reactivate`    | Fatto   | Solo Reason=User, password con lockout → 200 + token. Sospeso → 403 `account_suspended`. Account attivo → 200 + token                                                                                 |
| `DeleteUserByEmail`           | Rimosso | Sostituito da `/suspend` (decisione 2). Il FE non lo usava                                                                                                                                            |
| `UsersList`                   | Fatto   | `GetUserDto` con `Id`, `DeactivationReason` (`"User"`/`"Admin"`), `DeactivationNote`, `DeactivatedAt`: il backoffice può chiamare `/suspend`                                                          |

- Errori di autenticazione in formato `ProblemDetails` con estensione `code`
  stabile: `invalid_credentials`, `account_deactivated`, `account_suspended`,
  `cannot_suspend_self`, `cannot_suspend_admin`, `not_suspended`.
- La nota di sospensione non viene mai restituita al cliente (solo in
  `UsersList` per l'Admin).
- Test: 77 → 111. Warning di build: 109 (invariati).
- Terminologia: "ban" rinominato in "sospensione" (`/suspend`, `/unsuspend`) su
  richiesta; i commit già pushati sono stati allineati con un commit `refactor`
  separato.

### Impatto FE (M4)

- Login: 401 con corpo `ProblemDetails`; nuovi 403 da gestire tramite `code`:
  `account_deactivated` → pagina di riattivazione con conferma, poi pagina di
  benvenuto; `account_suspended` → messaggio di negazione (testo da definire).
- Nuovi endpoint: `POST /Account/reactivate`, `DELETE /Account/{id}` (campo
  motivo facoltativo), `PUT /Account/{id}/suspend` (nota obbligatoria) e
  `/unsuspend` nel backoffice, con l'`id` preso da `UsersList`.
- Dopo 5 password errate: 15 minuti di 401 anche con la password corretta.

### Limiti noti

- **Migration non verificata da CI** (SQLite): backfill e Down vanno provati su
  SQL Server prima del deploy.
- Lockout sul login: chi conosce un'email può bloccare quell'account per 15
  minuti (DoS mirato), compromesso standard.
- Il controllo del token legge l'utente a ogni richiesta autenticata.

### Fuori scope (non toccati)

1. **Canale laterale di tempo nel login**: con email inesistente la risposta è
   più veloce (nessuna verifica dell'hash), quindi i tempi possono rivelare se
   un'email è registrata. Soluzione: quando l'email non esiste, verificare
   comunque la password contro un hash fittizio
   (`PasswordHasher.VerifyHashedPassword` su un hash precalcolato) per rendere i
   tempi uguali.
2. Gestione ruoli e protezione "ultimo admin": milestone M6 (sopra).
3. `DateTime.Now` nella scadenza del token (#15, M3d).
4. Chiave di configurazione `SignInRequireConfirmedAccount` in
   `appsettings.json` contro `RequireConfirmedAccount` letta in `Program.cs`:
   non coincidono, l'impostazione è ignorata (oggi vale comunque false).

## Nuovo milestone: M6

- Gestione admin (milestone futura dopo M4, "M6"): oggi nessuna gestione ruoli
  via API; admin creati via DB. Proposta: ruolo SuperAdmin (GUID fisso); primo
  SuperAdmin da seed configurato via User Secrets/env o SQL documentato; POST
  /Account/admins solo SuperAdmin crea un Admin senza password e invia email di
  invito con token Identity a scadenza (24h, monouso); POST
  /Account/set-password; regole: Admin non crea admin, SuperAdmin non
  sospendibile/declassabile via API, protezione ultimo SuperAdmin, sospensione
  Admin solo da SuperAdmin. Prerequisito: infrastruttura email (IEmailSender;
  Mailpit/smtp4dev in sviluppo; SMTP/servizio esterno in produzione, credenziali
  via env). Stessa infrastruttura abilita "password dimenticata" per i clienti.
  README: documentare come creare il primo admin.

## Stato avanzamento M3b – gestione errori (aggiornato 2026-11-24)

Branch `fix/m3b-error-handling`, test-first, un controller per coppia red → fix.
Chiude #11 e generalizza il formato `ProblemDetails` + `code` introdotto dalla
fetta account a tutta l'API.

| Voce                                               | Stato  | Come                                                                                                                                                                         |
| -------------------------------------------------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 11 Try/catch con 500 generico (39 in 7 controller) | Chiuso | `Errors/GlobalExceptionHandler` (`IExceptionHandler` + `AddProblemDetails` + `UseExceptionHandler`). 0 `catch (Exception)` nei controller, 0 log + rethrow nei service (erano 10) |
| Formato degli errori                               | Chiuso | Unico: `ProblemDetails` con `code` stabile e `traceId` (`Errors/ApiProblems`), anche per validazione (`validation_failed`) e `NotFound()` senza corpo (`not_found`)           |
| 500 senza dettagli interni                         | Chiuso | `internal_error` senza `detail`: nessuno stack, messaggio SQL o testo dell'eccezione, in Testing, Development e Production. La developer exception page non viene mai mostrata |
| Log una sola volta                                 | Chiuso | Solo il gestore logga (Warning per gli errori di dominio, Error con stack per gli altri); il log di `ExceptionHandlerMiddleware` è silenziato in Serilog                     |
| Carrello vuoto al checkout                         | Chiuso | 409 `cart_empty` (era 500). Test di caratterizzazione invertito                                                                                                              |
| Sottocategoria/produttore inesistenti nel prodotto | Chiuso | 400 `subcategory_not_found` / `manufacturer_not_found` in create e update (erano 500)                                                                                        |
| Delete con elementi collegati                      | Chiuso | 409 `resource_in_use` per Category, SubCategory, Manufacturer (erano 500). Verifica esplicita nei service, indipendente dal provider                                        |
| Liste vuote = 404                                  | Chiuso | 200 `[]`: search, `GET /Product`, prodotti per categoria/sottocategoria/produttore, `GET /Order`, `/Order/user/{email}`                                                      |
| Avvio fallito con exit code 0                      | Chiuso | `Log.Fatal` con lo stack ed exit code 1. `HostAbortedException` (strumenti EF) non più loggata come errore                                                                   |

**Comportamento voluto – 409 `resource_in_use`**: una categoria, sottocategoria
o produttore non si può eliminare se ha prodotti collegati, **compresi quelli
eliminati** (soft delete): restano nel DB perché mantengono lo storico ordini.
Il `detail` della risposta lo dice esplicitamente.

**Codici** (`code`): `internal_error`, `validation_failed`, `not_found`,
`bad_request`, `unauthorized`, `forbidden`, `conflict` (generici per status);
`product_not_found`, `insufficient_stock`, `stock_conflict`, `cart_empty`,
`subcategory_not_found`, `manufacturer_not_found`, `resource_in_use`,
`invalid_quantity`, `email_change_not_allowed`, `email_already_registered`,
`weak_password`, `registration_failed`, `already_authenticated`, più quelli
della fetta account (`invalid_credentials`, `account_deactivated`,
`account_suspended`, `cannot_suspend_self`, `cannot_suspend_admin`,
`not_suspended`). Gli status dei casi già gestiti in M2/M3a non cambiano.

- Test: 111 → 164 (cartella `ErrorHandling/`). Warning di build: 109
  (invariati).
- Le politiche di Review di M1 restano nel controller (update altrui → 403,
  delete altrui → 404): sono gli unici due catch rimasti.

### Impatto FE (M4)

- Il FE oggi non legge mai il corpo degli errori (in ogni ramo `!response.ok`
  lancia un `Error` con testo fisso), quindi il nuovo formato non lo rompe. Per
  messaggi mirati può leggere `code` dal `ProblemDetails`.
- **Le liste vuote ora rispondono 200 `[]`** (prima 404, che il FE mostrava
  come errore): search, catalogo, prodotti per categoria/sottocategoria/
  produttore, ordini.
- **`cart_empty` è 409** al checkout (prima 500).

### Limiti noti

- **Ramo SQL Server 547 non testato**: la rete di sicurezza del gestore
  (`DbUpdateException` con errore 547 durante una delete → 409
  `resource_in_use`, riconosciuta dal numero e non dal testo) non si riproduce
  su SQLite. La coprirà il branch Testcontainers su SQL Server, insieme al race
  sullo stock. Gli altri 547 (insert/update con riferimenti inesistenti) non
  sono "risorsa in uso" e restano 500.
- **Email nei log**: il gestore logga metodo e `Request.Path`, e alcune route
  contengono ancora l'email (`/Cart/{email}`, `/Order/user/{email}`). Non è un
  problema nuovo (email nell'URL, già previsto per M4 e per la migrazione da
  email a id), ma il gestore centralizzato lo rende sistematico: ogni errore su
  quelle route scrive l'email nei log.
- In `ProductController` (`GetAll`, `GetProduct`, `SearchProducts`) è rimasto il
  blocco `{ }` che apriva il `try`, per non reindentare il mapping: sparisce con
  la centralizzazione dei mapping (#13, M3d). Il controllo della query vuota in
  `SearchProducts` è difensivo: la validazione risponde già 400
  `validation_failed`.

### Fuori scope emersi in M3b (non toccati)

1. **SubCategory con `categoryId` inesistente** in create/update: violazione FK →
   500 `internal_error`. Proposta: verifica nel service → 400
   `category_not_found`, come per prodotto e produttore.
2. **Integrità categoria/prodotto**: `Product` ha sia `SubCategoryId` sia
   `CategoryId` (dato duplicato). Spostare una SubCategory in un'altra Category
   lascia i prodotti con il vecchio `CategoryId`. Proposta: rimuovere
   `Product.CategoryId` e ricavarla dalla SubCategory (migration + query di
   filtro per categoria); in alternativa aggiornare i prodotti allo
   spostamento. Da pianificare in M3c/M3d.
3. Già previsti in M3c, ora nel formato unico: Review con `ProductId`
   inesistente → 500 `internal_error`; restore di un ordine con stock
   insufficiente → 404 `not_found`.

## Stato avanzamento M3c1 – carrello e ordini (aggiornato 2026-11-24)

Branch `fix/m3c1-carts-orders`, test-first (piano in `docs/PIANO-M3c1.md`). Chiude
i fuori scope M2 n. 3, 10, 11 e il punto "identità dal token" per carrello/ordini.

| Voce                           | Stato  | Come                                                                                                                                                                                  |
| ------------------------------ | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Route con identità dal token   | Chiuso | `GET /Cart/me`, `POST /Cart/me/add`, `DELETE /Cart/me/clear`, `GET /Order/me`, `POST /Order/me/from-cart`. Le route con `{email}` restano identiche e sono `[Obsolete]` (logica condivisa) |
| Annulla/ripristina ordine      | Chiuso | Flag ordine + stock in una transazione; `UPDATE … SET Quantity = Quantity ± n` atomici, flag con `WHERE IsDeleted = atteso` (niente doppio ripristino)                                |
| Restore senza stock sufficiente | Chiuso | **409 `insufficient_stock`** (era 404), tutto o niente                                                                                                                                |
| `OrderNumber` unique           | Chiuso | Indice unique, migration `UniqueOrderNumber` (colonna `nvarchar(max)` → `nvarchar(450)`, necessario per l'indice)                                                                     |
| `CustomerComment`              | Chiuso | Corpo facoltativo `{ "customerComment": "…" }` (max 500) su `from-cart` e `me/from-cart`; senza corpo `null` (prima stringa fissa)                                                    |

- Test: 164 → 188. Warning di build: 109 (invariati).
- **FE (M4)**: passare alle route `/me`; gestire 409 `insufficient_stock` al
  ripristino; il FE non usava `customerComment`.
- Migration provata sul DB SQL Server locale: backup con `sqlcmd` (verificato con
  `RESTORE VERIFYONLY`), `database update`, Down, Up. `AspNetUserRoles` = 5, ruoli
  = 3, ordini = 5 invariati ad ogni passo; nessuna operazione su `AspNetRoles`.
- `sqlcmd` moderno installato con `winget install sqlcmd` (non richiede ODBC Driver
  17). Con il servizio locale si connette solo via named pipe esplicita:
  `-S 'np:\\.\pipe\MSSQL$SQLEXPRESS\sql\query' -E -C`.

### Fuori scope (non toccati)

1. Collisione di `OrderNumber` (8 caratteri di GUID): ora un errore invece di un
   duplicato silenzioso; la generazione non cambia.

## Stato avanzamento M3c2 – catalogo, review, account (aggiornato 2026-11-24)

Branch `fix/m3c2-catalog-reviews-account`, test-first (piano in
`docs/PIANO-M3c2.md`). Chiude i fuori scope M3b n. 1, 2, 3 e i finding su review ed
email.

| Voce                                 | Stato  | Come                                                                                                                                                                                               |
| ------------------------------------ | ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SubCategory con categoria inesistente | Chiuso | 400 `category_not_found` in create/update (era 500)                                                                                                                                                |
| `Product.CategoryId` duplicato       | Chiuso | Rimosso: la categoria è `SubCategory.CategoryId`. Filtro `GET /Product/category/{id}` via sottocategoria, `category` nelle risposte derivato (stessa forma). Migration `RemoveProductCategoryId` |
| Review con prodotto inesistente      | Chiuso | 404 `product_not_found` (anche prodotto eliminato); era 500                                                                                                                                        |
| Rating review                        | Chiuso | `[Range(1, 5)]` su create e update → 400 `validation_failed`                                                                                                                                       |
| Email nelle review                   | Chiuso | `customerEmail` non esce più dalle risposte prodotto (`ProductReviewDto`). `GET /Review` e `/Review/{id}` (Admin/Seller) la mantengono per la moderazione                                         |
| Email unica                          | Chiuso | `RequireUniqueEmail = true`                                                                                                                                                                        |
| Mapping prodotto                     | Fatto  | `Mapping/ProductMapper`: un solo punto al posto di 5 copie (comportamento invariato; `manufacturerId` annidato resta 0 fino a M3d)                                                                |

- Test: 188 → 203. Warning di build: 109 → 108 (il refactor del mapping ne ha tolto uno).
- **FE (M4)**: nessun uso di review né di `customerEmail` nel FE attuale; la
  forma delle risposte prodotto non cambia, tranne l'assenza di `customerEmail`
  nelle review.
- Migration provata sul DB SQL Server locale: backup con `sqlcmd` (verificato),
  `database update`, Down, Up. `AspNetUserRoles` = 5, ruoli = 3, prodotti = 12
  invariati; nessuna operazione su `AspNetRoles`. Il Down ripopola `CategoryId`
  dalla sottocategoria prima di ricreare la FK (0 incoerenze dopo il Down). Il DB
  locale non aveva prodotti con categoria diversa da quella della sottocategoria.

### Fuori scope (non toccati)

1. Nessun indice unique su `AspNetUsers.NormalizedEmail`: `RequireUniqueEmail` è
   solo validazione di Identity.
2. Se un DB di produzione ha email duplicate, dopo `RequireUniqueEmail` l'update
   di quegli utenti fallisce: verificare prima del deploy.

## Stato avanzamento M3d – pulizia e performance (aggiornato 2026-11-24)

Branch `fix/m3d-cleanup` (piano in `docs/PIANO-M3d.md`). Chiude #12, #13 e i
minori di #15 che erano in M3; i refactor puri sono coperti dai test esistenti.

| Voce                                    | Stato  | Come                                                                                                                                                                                      |
| --------------------------------------- | ------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 13 Mapping duplicato                    | Chiuso | `Mapping/*` (Product, Order, Cart, Category, SubCategory, Manufacturer, Review): un solo punto per risorsa. Dove il mapping sta in una query è un'`Expression` (tradotta in SQL)           |
| `manufacturer.manufacturerId` sempre 0  | Chiuso | Test rosso → `ProductMapper`. `GET /Product/{category,subcategory,manufacturer}/…` ora usano `ManufacturerDto` (campo `manufacturerId` aggiunto: cambio additivo)                         |
| DTO morti                               | Chiuso | Rimossi `UserDto`, `CartDto`, `CreateOrderDto`, `OrderDto`, `OrderItemDto`, `AddSubCategoryDto`                                                                                           |
| 12 Query                                | Chiuso | `AsNoTracking` su tutte le letture (verificato sul change tracker); ordini per email: 5 include → 1; prodotti senza `Reviews.User`                                                        |
| Paginazione                             | Chiuso | `GET /Product` e `GET /Order`: `page`/`pageSize` opzionali (default 20, max 100), stesso array ordinato per id + header `X-Total-Count` (esposto via CORS). Senza parametri risposta invariata; valori non validi → 400 `validation_failed` |
| `Cart.TotalPrice`                       | Chiuso | Colonna rimossa, totale calcolato dalle righe (anche in cancellazione prodotto). Migration `RemoveCartTotalPrice` (Down ripopola il totale)                                               |
| Getter carrello che scrive              | Chiuso | `GetCartByEmailAsync` non aggiunge più un `Cart` al context (carrello vuoto per chi non ne ha); verifica "l'articolo è mio" con `Any` invece di caricare il carrello                       |
| 15 Minori                               | Chiuso | `DateTime.UtcNow` nella scadenza del token (nessun test: stesso istante), titolo Swagger "VTInformatica API" (con test), `Log/` già coperta da `.gitignore` (`[Ll]og/`)                    |
| Trim su `Name`/`Description`            | Chiuso | Create/update prodotto (`FullDescription` incluso). Dati esistenti: `docs/sql/correggi-dati-catalogo.sql` (nome con spazi, "Perferiche"), da eseguire a mano                              |
| Avvisi di build                         | Chiuso | **109 → 0** senza disattivare i controlli: stringhe/collezioni con valore iniziale, `null!` solo per le proprietà di navigazione EF e per i corpi di richiesta (un campo mancante resta un 400) |
| Commenti storici                        | Chiuso | Commit `chore: remove historical comments` (solo commenti); regola aggiunta a `CLAUDE.md`                                                                                                  |

- Test: 164 (inizio M3c) → **240**. Warning di build: **109 → 0**.
- **FE (M4)**: `X-Total-Count` per paginare; `manufacturerId` ora presente nei
  prodotti per categoria/sottocategoria/produttore; `GET /Cart/me` non risponde
  mai 404.
- Migration provata sul DB SQL Server locale con `sqlcmd` (backup verificato,
  `database update`, Down, Up): `AspNetUserRoles` = 5, ruoli = 3 invariati,
  nessuna operazione su `AspNetRoles`. **Limite**: il DB locale non ha righe in
  `CartItems`, quindi il ripopolamento del totale nel Down è stato eseguito solo
  su carrelli vuoti.

### Fuori scope (non toccati)

1. `AccountService` senza interfaccia; messaggi misti IT/EN.
2. Chiave `SignInRequireConfirmedAccount` in `appsettings.json` non letta da
   `Program.cs` (letta `RequireConfirmedAccount`).
3. `DatabaseScript.sql` (UTF-16) contiene ancora i dati con spazi e "Perferiche":
   non modificato, correggere i DB con lo script in `docs/sql/`.
4. `ProductService.UpdateAsync` risponde 404 quando le immagini del prodotto sono
   nulle o la lista nel corpo è vuota (condizione preesistente, resa solo
   null-safe).
5. Nessuna paginazione/ordinamento su altri elenchi (categorie, produttori,
   review).

## Chiusura M3 – verifiche (aggiornato 2026-11-24)

- **Down delle migration con dati reali**: `RemoveProductCategoryId` e
  `RemoveCartTotalPrice` ripopolano già le colonne ricreate (nessuna correzione
  necessaria). Provato su una copia del DB locale (restore da backup) con un
  carrello di 2 righe creato apposta: Down fino a `UniqueOrderNumber` →
  `TotalPrice` 6644,75 uguale al ricalcolo dalle righe, carrello vuoto a 0;
  `CategoryId` uguale a quello della sottocategoria per tutti i 12 prodotti, FK
  ricreata, `AspNetUserRoles` = 5. Copia eliminata dopo la prova.
- **Email duplicate**: 0 sul DB locale (5 utenti).
- **Dati catalogo**: `docs/sql/correggi-dati-catalogo.sql` eseguito sul DB locale
  dopo backup verificato: prodotto 18 senza spazi iniziali, categoria 2
  "Periferiche".

### Controlli obbligatori prima di un deploy

1. **Email duplicate** (con `RequireUniqueEmail` l'update di quegli utenti
   fallisce): deve restituire 0 righe.

   ```sql
   SELECT NormalizedEmail, COUNT(*) FROM AspNetUsers GROUP BY NormalizedEmail HAVING COUNT(*) > 1
   ```

2. Backup con `sqlcmd` (verificato con `RESTORE VERIFYONLY`) prima di
   `dotnet ef database update`.
3. `docs/sql/correggi-dati-catalogo.sql` sui DB creati da `DatabaseScript.sql`
   (idempotente).

## Modifiche BE richieste da M4 (FE) – aggiornato 2026-11-24

- **CORS `X-Total-Count`**: già esposto da M3d (`WithExposedHeaders`, test
  `TotalCountHeader_IsExposedToTheBrowser` con `Origin`). Nessuna fetta
  `fix/cors-expose-total-count` necessaria.
- **`feat/review-mine`** (piano in `docs/PIANO-review-mine.md`): `GET /Review/me`
  `[Authorize]` restituisce le recensioni dell'utente del token (`reviewId`,
  `productId`, `reviewRating`, `reviewComment`, `createdAt`), dalla più recente,
  senza email. Serve al FE per offrire modifica ed eliminazione solo sulle
  proprie. Test-first: 240 → 243. Warning: 0.

## Stato avanzamento – upgrade a .NET 10 LTS (aggiornato 2026-11-25)

Branch `chore/dotnet10`. .NET 8 esce dal supporto a novembre 2026.

| Voce                  | Stato  | Come                                                                                                                                                                                                 |
| --------------------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Target e pacchetti    | Chiuso | `net10.0` in entrambi i progetti. `Microsoft.*` 8.0.14 → 10.0.15, `Swashbuckle.AspNetCore` 6.6.2 → 10.3.1, `Serilog.AspNetCore` 9.0.0 → 10.0.0, xUnit 2.9.3, `dotnet-ef` 10.0.15. CI con `setup-dotnet` 10.0.x |
| Swagger               | Chiuso | Swashbuckle 10 usa Microsoft.OpenApi 2.x: `OpenApiSecurityScheme.Reference` rimosso, requirement via `OpenApiSecuritySchemeReference`. Nuovo test sullo schema Bearer (+2 test)                        |
| Test: EF 9+ e provider | Chiuso | `AddDbContext` registra anche `IDbContextOptionsConfiguration<T>`: la factory dei test lo rimuove, altrimenti SQL Server e SQLite risultano registrati insieme                                         |
| Test fragile          | Chiuso | Gli id "inutilizzati" di `ProductErrorTests` (6, 30, 21) dipendevano dall'ordine di esecuzione sul DB condiviso; ora sono id che nessun test crea                                                   |
| Model e migration     | Chiuso | `has-pending-model-changes` pulito: EF 10 non genera migration di normalizzazione, `AspNetRoles` e dati non toccati                                                                                    |
| DB locale             | Chiuso | Backup con `sqlcmd`, `database update` (nessuna migration da applicare), conteggi invariati (utenti 5, ruoli 3, ruoli utente 5, prodotti 12, ordini 5). API avviata, E2E del FE 8/8 verdi              |

- Test: 243 → 245. Avvisi di build: 0.
- Breaking change lette e non applicabili: `WithOpenApi` deprecato, `WebHost` obsoleto,
  `IPNetwork`, cookie login redirect, `Application Name` nella connection string
  (solo informativo), parametri multipli per `Contains()` (una sola query, `OrderService`: gli id dei prodotti del
  carrello, poche unità; risultati invariati). `IExceptionHandler` che ritorna `true` sopprime la diagnostica: i test del gestore
  errori restano verdi.

## Stato avanzamento – rimozione delle route `{email}` (aggiornato 2026-11-25)

Branch `chore/remove-email-routes`. Prima della rimozione: `grep` su `../vtinformatica_fe/src` e `e2e`
senza risultati per `Cart/{email}`, `Order/user/`, `from-cart/{email}` (il FE usa solo `/me` e le route per id).

| Voce                               | Stato  | Come                                                                                                                                                                         |
| ---------------------------------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Route `[Obsolete]` con `{email}`   | Chiuso | Rimosse `GET /Cart/{email}`, `POST /Cart/{email}/add`, `DELETE /Cart/{email}/clear`, `GET /Order/user/{email}`, `POST /Order/from-cart/{email}`. Test rosso: ora rispondono 404 |
| Test sulle route rimosse           | Chiuso | Quelli usati come mezzo (setup, lettura) passano a `/me`. Rimossi i 5 che verificavano la proprietà tramite l'email di un altro utente: con l'identità solo dal token il caso non esiste più. `CreateFromCart_OtherUsersCartStaysUntouched` verifica che B non tocchi il carrello di A |
| Lettura ordini di altri (Admin)    | Chiuso | Non più via email: restano `GET /Order` (paginato, staff) e `GET /Order/{id}`                                                                                                |
| Chiave email → `UserId` nel DB     | Aperto | Carrello e ordini sono ancora legati all'email (`Cart.Email`, `Order.CustomerEmail`): migrazione con backfill, fuori da questa fetta                                          |

- Test: 245 → 240 (−5 test sulla proprietà via email; i 5 casi `[Obsolete]` sono diventati 5 casi "route assenti"). Avvisi: 0.

## Stato avanzamento – test di integrazione su SQL Server (aggiornato 2026-11-25)

Branch `test/sqlserver-integration`. Progetto `VTInformatica.IntegrationTests` (fuori da `VTInformatica.sln`).

| Voce                                  | Stato  | Come                                                                                                                                                                                                                  |
| ------------------------------------- | ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Race reale sullo stock                | Chiuso | 12 checkout paralleli su uno stock di 7 (quantità 2) via HTTP, SQL Server 2022 in container: esattamente 3 riusciti, 9 × 409 `insufficient_stock`, stock finale 1, mai negativo. Mutando `Quantity >= qty` in `>= 0` il test fallisce (4 riusciti) |
| Ramo SQL Server 547 non testato       | Chiuso | Un interceptor inserisce una sottocategoria con un'altra connessione subito prima della `DELETE FROM [Categories]`: SQL Server dà 547 e il gestore risponde 409 `resource_in_use`. Un secondo test copre la strada normale (verifica nel service) |
| Migration verificate in CI            | Chiuso | Il DB di test si crea con le migration vere (`Migrate`, non `EnsureCreated`). Un test fa `Down` fino a `0` e `Up` di nuovo su DB vuoto; nel job CI anche `dotnet ef migrations has-pending-model-changes`                  |
| Unicità di `OrderNumber`              | Chiuso | Due ordini con lo stesso numero: errore SQL 2601 dall'indice unique creato dalla migration                                                                                                                            |
| `dotnet test` veloce, senza Docker    | Chiuso | Il progetto non è nella soluzione: gli Integration si lanciano con `dotnet test VTInformatica.IntegrationTests`. Senza Docker falliscono con un messaggio chiaro (mai skip). `INTEGRATION_SQLSERVER` permette di usare un server esistente |
| CI                                    | Chiuso | Job `integration` separato (ubuntu, Docker)                                                                                                                                                                           |

- Provati in locale su SQL Server Express (senza Docker, che sul PC non c'è): 5/5 verdi. Primo run nel container solo in CI.
- Non coperto: la corsa tra `Restore` e `Cancel` dello stesso ordine su SQL Server (resta il test deterministico su SQLite); deadlock con carrelli multi-prodotto.
