# Piano M3c1 – carrello e ordini

Branch `fix/m3c1-carts-orders`, test-first (red → fix per ogni coppia).

## Coppie

1. **Route `/me`** – `GET /Cart/me`, `POST /Cart/me/add`, `DELETE /Cart/me/clear`,
   `GET /Order/me`, `POST /Order/me/from-cart`: identità solo dal token. Le vecchie
   route con `{email}` restano identiche, marcate `[Obsolete]` (Swagger le mostra
   deprecate). La logica è condivisa tra vecchia e nuova route (nessuna copia).
2. **Annulla/ripristina ordine atomici** – flag ordine e stock nella stessa
   transazione; stock con `UPDATE … SET Quantity = Quantity ± n` (annulla) e
   `… WHERE Quantity >= n` (ripristina), in ordine di `ProductId`. Il flag
   dell'ordine cambia con `UPDATE … WHERE IsDeleted = <atteso>`: una seconda
   richiesta uguale non rimette stock due volte. Restore con stock insufficiente →
   **409 `insufficient_stock`** (prima 404), tutto o niente.
3. **`OrderNumber` unique** – indice unique + migration `UniqueOrderNumber`.
4. **`CustomerComment`** – non più hardcoded: corpo opzionale
   `{ "customerComment": "…" }` (max 500) su `from-cart` e `me/from-cart`; senza
   corpo il commento è `null`.

## Retrocompatibilità HTTP

Route vecchie e risposte invariate. Cambiano solo: restore con stock insufficiente
404 → 409 (richiesto esplicitamente) e `customerComment` `null` invece della
stringa fissa (nessun uso nel FE).

## Migration

Solo l'indice unique su `Orders.OrderNumber`. Prima: `has-pending-model-changes`
pulito, nessuna operazione su `AspNetRoles`, Up/Down rivisti. Prova sul DB locale
solo se il backup con `sqlcmd` riesce (vedi audit).

## Fuori scope

Collisione di `OrderNumber` (8 caratteri di GUID): con l'indice unique una
collisione diventa un errore invece di un duplicato silenzioso; la generazione non
cambia.
