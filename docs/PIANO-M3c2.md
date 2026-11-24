# Piano M3c2 – catalogo, review, account

Branch `fix/m3c2-catalog-reviews-account`, test-first (red → fix per ogni coppia).

## Coppie

1. **SubCategory con `categoryId` inesistente** – create/update → 400
   `category_not_found` (nuova `CategoryNotFoundException`, stesso schema di
   `SubCategoryNotFoundException`). Era 500.
2. **Review** – `POST /Review` con `ProductId` inesistente (o eliminato) → 404
   `product_not_found` (era 500); `ReviewRating` validato `[Range(1, 5)]` su create
   e update → 400 `validation_failed`; `customerEmail` non più esposta nelle review
   dentro le risposte prodotto (pubbliche). `GET /Review` e `GET /Review/{id}`
   (solo Admin/Seller) mantengono il campo: serve alla moderazione.
3. **`RequireUniqueEmail`** – attivo in Identity (due utenti con la stessa email,
   anche con UserName diverso, non sono più possibili).
4. **Rimozione di `Product.CategoryId`** – la categoria si ricava da
   `SubCategory.CategoryId`. Migration `RemoveProductCategoryId` (drop FK, indice,
   colonna; nel Down la colonna viene ripopolata dalla sottocategoria prima di
   ricreare la FK). Filtro "prodotti per categoria" riscritto
   (`p.SubCategory.CategoryId`). Il campo `category` nelle risposte non cambia
   (derivato). Il mapping prodotto → DTO, copiato 5 volte, passa in un solo punto
   (`Mapping/ProductMapper`) perché va comunque toccato; resta identico al
   comportamento attuale (il fix di `manufacturerId` = 0 è M3d, con il suo test).

## Retrocompatibilità HTTP

Invariati: route, forma delle risposte prodotto (`category` incluso). Cambiano
(richiesti): 500 → 400/404 negli input sbagliati; `customerEmail` sparisce dalle
review dentro `GET /Product*`; rating fuori 1–5 → 400.

## Migration

Prima: `has-pending-model-changes` pulito, nessuna operazione su `AspNetRoles`,
Up/Down rivisti, backup con `sqlcmd`, `database update`, Down, Up, conteggio
`AspNetUserRoles` invariato. Il DB locale non ha prodotti con `CategoryId` diverso
dalla categoria della sottocategoria.

## Fuori scope

- Indice unique su `AspNetUsers.NormalizedEmail` (RequireUniqueEmail è solo
  validazione di Identity).
- Moderazione delle review da parte dell'Admin (già fuori da M3).
