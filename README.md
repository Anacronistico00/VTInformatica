# 🖥️ VT Informatica

**VT Informatica BE** è il back-end ufficiale del progetto VT Informatica, una piattaforma di e-shop moderna e reattiva progettata per offrire un'esperienza utente fluida e intuitiva.

## Qui il link per il FrontEnd
- [Link alla repo del FrontEnd](https://github.com/Anacronistico00/vtinformatica_fe)
  
## 🚀 Tecnologie Utilizzate

- **ASP.NET Core**: Framework per lo sviluppo di applicazioni web robuste e scalabili.
- **Entity Framework Core**: ORM per la gestione delle operazioni di accesso ai dati e mappatura degli oggetti.
- **SQL Server**: Sistema di gestione di database relazionali.
- **JWT (JSON Web Token)**: Per l'autenticazione e l'autorizzazione sicura degli utenti.
- **Serilog**: Per il logging avanzato e la gestione dei log.
- **AutoMapper**: Per la mappatura degli oggetti e la gestione delle conversioni tra DTO e entità.

## ⚙️ Configurazione

### Chiave JWT

La chiave di firma dei token **non è nel repository**. All'avvio l'API verifica che `Jwt:SecurityKey` sia presente e lunga almeno 32 caratteri; altrimenti non parte.

**Sviluppo** (User Secrets, salvati fuori dal repository):

```bash
dotnet user-secrets set "Jwt:SecurityKey" "<chiave di almeno 32 caratteri>" --project VTInformatica
```

**Produzione** (variabile d'ambiente; `__` sostituisce `:`):

```bash
Jwt__SecurityKey=<chiave di almeno 32 caratteri>
```

> La chiave usata in passato è presente nella storia git di questo repository pubblico: va considerata compromessa e **non deve essere riutilizzata**.

`Jwt:Issuer`, `Jwt:Audience` e `Jwt:ExpiresInMinutes` restano in `appsettings.json`.

> Dopo il deploy di questa versione gli utenti devono **rifare il login**: i token emessi in precedenza non contengono il claim `NameIdentifier` (id utente), necessario per modificare il profilo e per modificare o cancellare le proprie recensioni.

### CORS

Le origini ammesse sono in `Cors:AllowedOrigins` (`appsettings.json`, di default `http://localhost:5173` per il FrontEnd in sviluppo). In produzione si possono impostare con variabili d'ambiente, ad esempio:

```bash
Cors__AllowedOrigins__0=https://<dominio-del-frontend>
```

## 📬 Contatti

- 📧 Email: viktoryturiaci2323@gmail.com
- 💼 LinkedIn: [Vittorio Turiaci](https://www.linkedin.com/in/vittorio-turiaci-2646a9333/)
