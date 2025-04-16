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

            var existingUser = await _userManager.FindByEmailAsync(registerRequestDto.Email);
            if (existingUser != null)
            {
                if (!existingUser.IsActive)
                {
                    existingUser.Email = registerRequestDto.Email;
                    existingUser.UserName = registerRequestDto.Email;
                    existingUser.FirstName = registerRequestDto.FirstName;
                    existingUser.LastName = registerRequestDto.LastName;
                    existingUser.BirthDate = registerRequestDto.BirthDate;
                    existingUser.IsActive = true;

                    var updateResult = await _userManager.UpdateAsync(existingUser);
                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                        return BadRequest(new { message = "Errore nella riattivazione dell’utente", errors });
                    }

                    return Ok(new { message = "Utente riattivato con successo." });
                }

                return BadRequest(new { message = "Email già registrata" });
            }

            var newUser = new ApplicationUser()
            {
                Email = registerRequestDto.Email,
                UserName = registerRequestDto.Email,
                FirstName = registerRequestDto.FirstName,
                LastName = registerRequestDto.LastName,
                BirthDate = registerRequestDto.BirthDate,
                IsActive = registerRequestDto.IsActive,
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

            await _userManager.AddToRoleAsync(newUser, "Admin");

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

            if (user == null || !user.IsActive)
            {
                return Unauthorized("Email o password non valide.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginRequestDto.Password, false, false);

            if (!result.Succeeded)
            {
                return Unauthorized("Invalid email or password");
            }

            var roles = await _signInManager.UserManager.GetRolesAsync(user);

            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"));
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
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto, string id)
        {
            var success = await _accountService.UpdateUserAsync(id, dto);
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
    }
}
