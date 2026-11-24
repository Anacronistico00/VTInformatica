 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VTInformatica.DTOs.Account;
using VTInformatica.Exceptions;
using VTInformatica.Models.Auth;
using VTInformatica.Services;
using VTInformatica.Settings;

namespace VTInformatica.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly Jwt _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly AccountService _accountService;

        public AccountController(IOptions<Jwt> jwtOptions, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, AccountService accountService)
        {
            _jwtSettings = jwtOptions.Value;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            if (User.Identity.IsAuthenticated)
            {
                return BadRequest(new { message = "Sei già loggato. Non puoi registrarti di nuovo." });
            }

            // AUDIT #9: anche un utente disattivato è "già registrato". Prima la registrazione ne
            // sovrascriveva il profilo senza password; la riattivazione non passa da qui.
            // Stessa risposta per attivi e disattivati: non rivela lo stato dell'account.
            var existingUser = await _userManager.FindByEmailAsync(registerRequestDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email già registrata" });
            }

            var newUser = new ApplicationUser()
            {
                Email = registerRequestDto.Email,
                UserName = registerRequestDto.Email,
                FirstName = registerRequestDto.FirstName,
                LastName = registerRequestDto.LastName,
                BirthDate = registerRequestDto.BirthDate,
                IsActive = true, // mai dal client (AUDIT #9)
            };

            // Verifico se la password soddisfa i criteri
            var passwordValidationResult = await _userManager.PasswordValidators.FirstOrDefault().ValidateAsync(_userManager, newUser, registerRequestDto.Password);
            if (!passwordValidationResult.Succeeded)
            {
                // Se la password non è valida, non crea l'utente e restituisce un errore
                return BadRequest(new
                {
                    message = "Password must include at least one uppercase letter, one lowercase letter, one number, and one special character (-_@!%)."
                });
            }

            var result = await _userManager.CreateAsync(newUser, registerRequestDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "User registration failed.", errors = result.Errors.Select(e => e.Description) });
            }

            await _userManager.AddToRoleAsync(newUser, "Customer");

            return Ok(new { message = "Registrazione effettuata con successo!" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {

            if (User.Identity.IsAuthenticated)
            {
                return BadRequest(new { message = "Sei già loggato." });
            }

            var user = await _userManager.FindByEmailAsync(loginRequestDto.Email);

            // Prima la password, poi lo stato: con password errata, email inesistente o lockout
            // la risposta è sempre la stessa, così lo stato dell'account non viene rivelato.
            // CheckPasswordSignInAsync controlla il lockout prima della password e non crea cookie.
            if (user == null)
            {
                return InvalidCredentials();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequestDto.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return InvalidCredentials();
            }

            if (!user.IsActive)
            {
                return AccountInactive(user);
            }

            var roles = await _signInManager.UserManager.GetRolesAsync(user);

            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"));
            claims.Add(new Claim(ClaimTypes.DateOfBirth, user.BirthDate.ToString("yyyy-MM-dd")));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddMinutes(_jwtSettings.ExpiresInMinutes);

            var token = new JwtSecurityToken(_jwtSettings.Issuer, _jwtSettings.Audience, claims, expires: expiry, signingCredentials: creds);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new TokenResponseDto()
            {
                Token = tokenString,
                Expires = expiry
            });
        }

        [HttpGet("UsersList")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _accountService.GetAllUsersAsync();
            return Ok(new GetUsersResponseDto()
            {
                message = "Utenti registrati trovati: ",
                users = users
            });
        }

        [HttpPut("UpdateUser/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto, string id)
        {
            // Solo il proprio profilo (id dal token), salvo Admin.
            if (id != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Admin"))
                return Forbid();

            bool success;
            try
            {
                success = await _accountService.UpdateUserAsync(id, dto);
            }
            catch (EmailChangeNotAllowedException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            if (!success)
                return NotFound(new { message = "Utente non trovato o errore nell'aggiornamento." });

            return Ok(new { message = "Utente aggiornato con successo." });
        }

        [HttpPut("DeleteUserByEmail/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var success = await _accountService.DeleteUserAsync(email);
            if (!success)
                return NotFound(new { message = "Utente non trovato o errore durante la cancellazione." });

            return Ok(new { message = "Utente eliminato con successo." });
        }

        // ---- Errori dell'autenticazione: ProblemDetails con un "code" stabile per il FE ----

        /// <summary>Unica risposta per email inesistente, password errata e lockout: nessuna enumerazione.</summary>
        private ObjectResult InvalidCredentials() =>
            AccountProblem(StatusCodes.Status401Unauthorized, "Email o password non valide.", "invalid_credentials");

        /// <summary>Solo dopo la verifica della password. La nota di disattivazione non viene mai restituita.</summary>
        private ObjectResult AccountInactive(ApplicationUser user) =>
            user.DeactivationReason == DeactivationReason.User
                ? AccountProblem(StatusCodes.Status403Forbidden, "Account disattivato.", "account_deactivated")
                : AccountProblem(StatusCodes.Status403Forbidden, "Account sospeso.", "account_suspended");

        private ObjectResult AccountProblem(int statusCode, string title, string code)
        {
            var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: statusCode, title: title);
            problem.Extensions["code"] = code;
            return new ObjectResult(problem) { StatusCode = statusCode };
        }
    }
}
