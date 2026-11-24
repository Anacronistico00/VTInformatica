using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;
using VTInformatica.Data;
using VTInformatica.DTOs;
using VTInformatica.Errors;
using VTInformatica.Interfaces;
using VTInformatica.Models.Auth;
using VTInformatica.Services;
using VTInformatica.Settings;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Syster", Serilog.Events.LogEventLevel.Warning)
    // Le eccezioni le logga GlobalExceptionHandler (una volta sola): il middleware di ASP.NET Core
    // le registrerebbe di nuovo come Error, anche quelle di dominio.
    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", Serilog.Events.LogEventLevel.Fatal)
    .Enrich.FromLogContext()
    .WriteTo.Async(a => a.File("Log/Log_txt", rollingInterval: RollingInterval.Day))
    .WriteTo.Async(a => a.Console())
    .CreateLogger();

try
{
    Log.Information("Starting application.....");

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();

    // AUDIT #11: errori in un unico formato (ProblemDetails + code + traceId) e un unico gestore.
    builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ApiProblems.Customize);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomersManager API", Version = "v1" });

        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Inserisci il token JWT nel formato: Bearer {token}",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        c.AddSecurityDefinition("Bearer", securityScheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securityScheme, new string[] {} }
        });
    });

    builder.Services.Configure<Jwt>(builder.Configuration.GetSection(nameof(Jwt)));

    // La chiave JWT non è in appsettings.json: arriva da User Secrets (Development)
    // o dalla variabile d'ambiente Jwt__SecurityKey. Il messaggio non contiene mai la chiave.
    var jwtKey = builder.Configuration["Jwt:SecurityKey"];
    if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    {
        throw new InvalidOperationException(
            "Configurazione mancante: Jwt:SecurityKey assente o più corta di 32 caratteri. " +
            "Impostarla con 'dotnet user-secrets set \"Jwt:SecurityKey\" <chiave>' (Development) " +
            "o con la variabile d'ambiente Jwt__SecurityKey.");
    }

    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount =
            builder.Configuration.GetSection("Identity").GetValue<bool>("RequireConfirmedAccount");

        options.Password.RequiredLength =
            builder.Configuration.GetSection("Identity").GetValue<int>("RequiredLength");

        options.Password.RequireDigit =
            builder.Configuration.GetSection("Identity").GetValue<bool>("RequireDigit");

        options.Password.RequireLowercase =
            builder.Configuration.GetSection("Identity").GetValue<bool>("RequireLowercase");

        options.Password.RequireNonAlphanumeric =
            builder.Configuration.GetSection("Identity").GetValue<bool>("RequireNonAlphanumeric");

        options.Password.RequireUppercase =
            builder.Configuration.GetSection("Identity").GetValue<bool>("RequireUppercase");

        // Carrello e ordini usano l'email come chiave: un'email = un utente.
        options.User.RequireUniqueEmail = true;

        // Lockout: usato da login e reactivate (CheckPasswordSignInAsync con lockoutOnFailure).
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts =
            builder.Configuration.GetSection("Identity").GetValue("MaxFailedAccessAttempts", 5);
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
            builder.Configuration.GetSection("Identity").GetValue("LockoutMinutes", 15));
    })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters()
           {
               ValidateIssuer = true,

               ValidateAudience = true,

               ValidateLifetime = true,

               ValidateIssuerSigningKey = true,

               ValidIssuer = builder.Configuration.GetSection(nameof(Jwt)).GetValue<string>("Issuer"),

               ValidAudience = builder.Configuration.GetSection(nameof(Jwt)).GetValue<string>("Audience"),

               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
           };

           // Firma e scadenza non bastano: il token di un utente disattivato o cancellato
           // non deve più autenticare (→ 401). Una lettura dell'utente per richiesta autenticata.
           options.Events = new JwtBearerEvents
           {
               OnTokenValidated = async context =>
               {
                   var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                   var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                   var user = userId == null ? null : await userManager.FindByIdAsync(userId);

                   if (user == null || !user.IsActive)
                   {
                       context.Fail("Utente inesistente o disattivato.");
                   }
               }
           };
       });


    builder.Services.AddScoped<UserManager<ApplicationUser>>();
    builder.Services.AddScoped<SignInManager<ApplicationUser>>();
    builder.Services.AddScoped<RoleManager<ApplicationRole>>();



    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

    builder.Services.AddScoped<AccountService>();

    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
    builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();

    builder.Host.UseSerilog();

    var app = builder.Build();

    // Primo middleware della pipeline: avvolge tutto il resto. In Development WebApplication
    // aggiunge da sé la developer exception page *fuori* da questo middleware: il gestore
    // intercetta ogni eccezione prima, quindi la pagina (con lo stack) non viene mai mostrata.
    // Verificato da GlobalErrorHandlerTests in Testing, Development e Production.
    app.UseExceptionHandler();

    // Solo le origini configurate in Cors:AllowedOrigins (audit #10).
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

    app.UseCors(c =>
        c.WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders(PageQuery.TotalCountHeader)
        );

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
// HostAbortedException non è un errore: è il modo in cui gli strumenti EF (migrations add,
// has-pending-model-changes) fermano l'host dopo averlo costruito. Va lasciata propagare.
catch (Exception ex) when (ex is not HostAbortedException)
{
    // Avvio fallito: log completo (stack compreso) ed exit code 1, così orchestratori e CI
    // vedono il fallimento (prima: solo ex.Message e uscita con 0).
    Log.Fatal(ex, "Avvio dell'applicazione fallito.");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;

// Esposto per WebApplicationFactory<Program> nei test di integrazione.
public partial class Program { }
