# Piano feat/review-mine – le mie recensioni

Richiesta dal FE (M4f): per offrire modifica ed eliminazione solo sulle proprie
recensioni, il FE deve sapere quali sono. Le review dentro le risposte prodotto non
hanno l'autore (non si espone l'email), quindi serve un endpoint dedicato.

- `GET /Review/me` `[Authorize]`: recensioni dell'utente del token (id dai claim,
  mai da route o body), ordinate dalla più recente. Campi: `reviewId`,
  `productId`, `reviewRating`, `reviewComment`, `createdAt`. Nessuna email.
- Nessun utente autenticato → 401. Nessuna recensione → 200 `[]`.
- Test-first (red → fix), nessuna migration, nessun cambio alle route esistenti.
