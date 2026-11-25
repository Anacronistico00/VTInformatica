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

-- Sottocategoria 3: "RAMs" -> "RAM" (sigla al singolare come le altre voci).
UPDATE SubCategories
SET Name = N'RAM'
WHERE Id = 3 AND Name = N'RAMs';

-- Loghi dei produttori. ASUS e Noctua avevano un URL che non risponde (404 / 429 ai client
-- automatici); ThermalRight (bianco) e Seagate (grigio chiaro) non si leggono su fondo chiaro.
UPDATE Manufacturers
SET ManufacturerLogo = N'https://upload.wikimedia.org/wikipedia/commons/8/86/Asus_blue_logo.svg'
WHERE ManufacturerId = 4 AND ManufacturerName = N'ASUS';

UPDATE Manufacturers
SET ManufacturerLogo = N'https://iconlogovector.com/uploads/images/2025/02/lg-67b068892fe4d-Thermalright.webp'
WHERE ManufacturerId = 8 AND ManufacturerName = N'ThermalRight';

UPDATE Manufacturers
SET ManufacturerLogo = N'https://upload.wikimedia.org/wikipedia/commons/6/68/Noctua_wordmark.svg'
WHERE ManufacturerId = 11 AND ManufacturerName = N'Noctua';

UPDATE Manufacturers
SET ManufacturerLogo = N'https://upload.wikimedia.org/wikipedia/commons/b/b6/Seagate-Logo.svg'
WHERE ManufacturerId = 13 AND ManufacturerName = N'Seagate';

COMMIT TRANSACTION;
