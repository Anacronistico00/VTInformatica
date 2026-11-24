# Piano M3d – pulizia e performance

Branch `fix/m3d-cleanup`, test-first dove cambia il comportamento; i refactor puri
sono coperti dai test esistenti (commit `refactor`/`chore`, nessun cambio HTTP).

## Commit, in ordine

1. **Carrello** – il getter non aggiunge più un `Cart` al context (restituisce un
   carrello vuoto se non esiste); `Cart.TotalPrice` rimosso (colonna ridondante,
   il totale si calcola dalle righe; migration `RemoveCartTotalPrice`); la verifica
   "l'articolo è mio" diventa una query `Any` invece di caricare l'intero carrello.
2. **Mapping** – `manufacturerId` annidato sempre 0 in `GET /Product*` (test rosso,
   fix in `ProductMapper`); mapping di ordine, carrello, review, categoria,
   sottocategoria e produttore in classi `Mapping/*` (un solo punto ciascuno).
3. **DTO morti** – `UserDto`, `CartDto`, `CreateOrderDto`, `OrderDto`,
   `AddSubCategoryDto`, `OrderItemDto`.
4. **Query (#12)** – `AsNoTracking` su tutte le letture; include ridotti a quelli
   mappati (ordini per email: 5 livelli → 1; prodotti senza `Reviews.User`).
5. **Paginazione opzionale** – `GET /Product` e `GET /Order` accettano `page` e
   `pageSize` (default 20, max 100). Senza parametri la risposta è identica a prima
   (array completo). Con parametri: stesso array, ordinato per `Id`, più header
   `X-Total-Count` (esposto via CORS). Valori non validi → 400 `validation_failed`.
6. **Trim** – `Name`, `Description`, `FullDescription` del prodotto vengono
   ripuliti da spazi iniziali/finali in create e update. Script per i dati esistenti
   (`docs/sql/correggi-dati-catalogo.sql`: nome prodotto con spazi, "Perferiche"),
   non eseguito dall'app né da migration.
7. **Minori (#15)** – `DateTime.UtcNow` nella scadenza del token, titolo Swagger
   "VTInformatica API" (con test), `.gitignore` già copre `Log/` (verificato).
8. **Avvisi di build** – da 108 verso 0 (tipi nullable corretti in DTO e modelli,
   senza disattivare i controlli).
9. **`chore: remove historical comments`** – solo commenti, nessuna modifica al
   codice; regola aggiunta a `CLAUDE.md`.
10. **docs** – stato nell'audit.

## Retrocompatibilità HTTP

Senza parametri tutte le risposte hanno la stessa forma di prima. Cambiano solo:
`manufacturer.manufacturerId` (da 0 al valore vero), Name/Description senza spazi
ai bordi.

## Migration

`RemoveCartTotalPrice`: drop della colonna `Carts.TotalPrice`. Down: la colonna
torna nullable e viene ripopolata dalle righe. Prima: `has-pending-model-changes`
pulito, nessuna operazione su `AspNetRoles`, backup con `sqlcmd`, Up/Down/Up e
conteggio `AspNetUserRoles`.

## Fuori scope

- `AccountService` senza interfaccia; chiave `SignInRequireConfirmedAccount` in
  `appsettings.json` ignorata; messaggi misti IT/EN.
- Ordinamento/filtri di catalogo e ordini oltre alla paginazione.
