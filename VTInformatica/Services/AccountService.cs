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
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = $"{user.FirstName} {user.LastName}",
                    BirthDate = user.BirthDate,
                    Role = role,
                    IsActive = user.IsActive,
                    DeactivationReason = user.DeactivationReason,
                    DeactivationNote = user.DeactivationNote,
                    DeactivatedAt = user.DeactivatedAt
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

        /// <summary>Riattivazione self-service (password già verificata dal controller): azzera i campi di disattivazione.</summary>
        public async Task ReactivateAsync(ApplicationUser user)
        {
            user.IsActive = true;
            user.DeactivationReason = null;
            user.DeactivationNote = null;
            user.DeactivatedAt = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("Riattivazione non salvata.");
        }

        /// <summary>
        /// Sospensione da Admin (sostituisce DeleteUserByEmail). Un utente già disattivato diventa sospeso;
        /// un sospeso aggiorna la nota. Gli Admin non sono sospendibili (gestione ruoli fuori scope).
        /// </summary>
        public async Task<AccountStatusChangeResult> SuspendAsync(string userId, string note)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return AccountStatusChangeResult.NotFound;

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return AccountStatusChangeResult.CannotSuspendAdmin;

            user.IsActive = false;
            user.DeactivationReason = DeactivationReason.Admin;
            user.DeactivationNote = note.Trim();
            user.DeactivatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? AccountStatusChangeResult.Done : throw new InvalidOperationException("Sospensione non salvata.");
        }

        /// <summary>Revoca solo una sospensione: chi si è disattivato da sé si riattiva con la password.</summary>
        public async Task<AccountStatusChangeResult> UnsuspendAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return AccountStatusChangeResult.NotFound;

            if (user.IsActive || user.DeactivationReason != DeactivationReason.Admin)
                return AccountStatusChangeResult.NotSuspended;

            user.IsActive = true;
            user.DeactivationReason = null;
            user.DeactivationNote = null;
            user.DeactivatedAt = null;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? AccountStatusChangeResult.Done : throw new InvalidOperationException("Revoca della sospensione non salvata.");
        }
    }
}
