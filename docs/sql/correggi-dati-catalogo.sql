-- Correzione dei dati del catalogo nei database creati da DatabaseScript.sql.
-- Idempotente: si può rieseguire. Non fa parte delle migration e non viene eseguito dall'app.
-- Prima di eseguirlo su un database con dati reali: fare un backup.

BEGIN TRANSACTION;

-- Nomi e descrizioni dei prodotti con spazi ai bordi (es. il prodotto 18 "    Intel Core i9-14900K ...").
UPDATE Products
SET Name = LTRIM(RTRIM(Name)),
    Description = LTRIM(RTRIM(Description)),
    FullDescription = LTRIM(RTRIM(FullDescription))
WHERE Name <> LTRIM(RTRIM(Name))
   OR Description <> LTRIM(RTRIM(Description))
   OR FullDescription <> LTRIM(RTRIM(FullDescription));

-- Refuso nel nome della categoria 2 ("Perferiche" -> "Periferiche").
UPDATE Categories
SET Name = N'Periferiche'
WHERE Id = 2 AND Name = N'Perferiche';

COMMIT TRANSACTION;
