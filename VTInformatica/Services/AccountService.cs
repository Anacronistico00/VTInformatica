using Microsoft.AspNetCore.Identity;
using VTInformatica.DTOs.Account;
using VTInformatica.Exceptions;
using VTInformatica.Models.Auth;

namespace VTInformatica.Services
{
    public class AccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<GetUserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();

            var result = new List<GetUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                result.Add(new GetUserDto
                {
                    Email = user.Email,
                    FullName = $"{user.FirstName} {user.LastName}",
                    BirthDate = user.BirthDate,
                    Role = role,
                    IsActive = user.IsActive
                });
            }

            return result;
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Carrello e ordini usano l'email come chiave: cambiarla qui romperebbe il legame
            // (e lascerebbe UserName disallineato). Il cambio email richiede un flusso dedicato.
            if (dto.Email != null && !string.Equals(dto.Email, user.Email, StringComparison.OrdinalIgnoreCase))
                throw new EmailChangeNotAllowedException();

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.BirthDate = dto.BirthDate;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        /// <summary>Il cliente disattiva il proprio account: potrà riattivarlo con la password.</summary>
        public async Task<bool> DeactivateSelfAsync(string userId, string? note)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = false;
            user.DeactivationReason = DeactivationReason.User;
            user.DeactivationNote = note;
            user.DeactivatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
