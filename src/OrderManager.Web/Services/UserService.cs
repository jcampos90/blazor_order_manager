using Microsoft.AspNetCore.Identity;
using OrderManager.Web.Auth;
using OrderManager.Web.Models;

namespace OrderManager.Web.Services;

public sealed class UserService
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<UserSummary>> ListAsync()
    {
        var users = _userManager.Users.ToList();
        var list = new List<UserSummary>(users.Count);
        foreach (var u in users.OrderBy(u => u.Email, StringComparer.OrdinalIgnoreCase))
        {
            var roles = await _userManager.GetRolesAsync(u);
            list.Add(new UserSummary(
                u.Id,
                u.Email ?? "",
                u.DisplayName,
                roles.FirstOrDefault() ?? "",
                u.MustChangePassword));
        }
        return list;
    }

    public async Task<UserSummary?> GetByIdAsync(string userId)
    {
        var u = await _userManager.FindByIdAsync(userId);
        if (u is null) return null;
        var roles = await _userManager.GetRolesAsync(u);
        return new UserSummary(
            u.Id,
            u.Email ?? "",
            u.DisplayName,
            roles.FirstOrDefault() ?? "",
            u.MustChangePassword);
    }

    public async Task<(string TempPassword, string UserId)> CreateAsync(
        string email, string? displayName, string role)
    {
        EnsureValidRole(role);

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            throw new InvalidOperationException($"Ya existe un usuario con el email {email}.");

        var tempPassword = GenerateTempPassword();
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
            MustChangePassword = true,
        };

        var createResult = await _userManager.CreateAsync(user, tempPassword);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            throw new InvalidOperationException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        return (tempPassword, user.Id);
    }

    public async Task UpdateAsync(string userId, string? email, string? displayName)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (email is not null)
        {
            var trimmed = email.Trim();
            if (!string.Equals(trimmed, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userManager.FindByEmailAsync(trimmed);
                if (existing is not null && existing.Id != user.Id)
                    throw new InvalidOperationException($"Ya existe un usuario con el email {trimmed}.");
                user.Email = trimmed;
                user.UserName = trimmed;
                user.NormalizedEmail = _userManager.NormalizeEmail(trimmed);
                user.NormalizedUserName = _userManager.NormalizeName(trimmed);
            }
        }

        if (displayName is not null)
        {
            user.DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task ChangeRoleAsync(string userId, string newRole)
    {
        EnsureValidRole(newRole);

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (await _userManager.IsInRoleAsync(user, newRole))
            return;

        await EnsureNotLastOwnerAsync(user, roleAfter: newRole);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", removeResult.Errors.Select(e => e.Description)));
        }
        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
            throw new InvalidOperationException(string.Join(", ", addResult.Errors.Select(e => e.Description)));
    }

    public async Task<string> ResetPasswordAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var newPassword = GenerateTempPassword();
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        user.MustChangePassword = true;
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
            throw new InvalidOperationException(string.Join(", ", update.Errors.Select(e => e.Description)));

        return newPassword;
    }

    public async Task<bool> DeleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        if (await _userManager.IsInRoleAsync(user, UserRoleInitializer.OwnerRole)
            && await CountOwnersAsync() <= 1)
        {
            return false;
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        return true;
    }

    public async Task ChangeOwnPasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var message = result.Errors.FirstOrDefault(e => e.Code == "PasswordMismatch") is not null
                ? "La contraseña actual no es correcta."
                : string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(message);
        }

        user.MustChangePassword = false;
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
            throw new InvalidOperationException(string.Join(", ", update.Errors.Select(e => e.Description)));
    }

    public async Task UpdateOwnDisplayNameAsync(string userId, string? displayName)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");
        user.DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<int> CountOwnersAsync()
    {
        var owners = await _userManager.GetUsersInRoleAsync(UserRoleInitializer.OwnerRole);
        return owners.Count;
    }

    private async Task EnsureNotLastOwnerAsync(AppUser user, string roleAfter)
    {
        var isCurrentlyOwner = await _userManager.IsInRoleAsync(user, UserRoleInitializer.OwnerRole);
        var willStillBeOwner = string.Equals(roleAfter, UserRoleInitializer.OwnerRole, StringComparison.Ordinal);

        if (isCurrentlyOwner && !willStillBeOwner && await CountOwnersAsync() <= 1)
        {
            throw new LastOwnerException(
                "No podés dejar al sistema sin Owners. Promové a otro usuario a Owner antes.");
        }
    }

    private static void EnsureValidRole(string role)
    {
        if (role != UserRoleInitializer.OwnerRole && role != UserRoleInitializer.StaffRole)
            throw new ArgumentException($"Rol desconocido: {role}", nameof(role));
    }

    private static string GenerateTempPassword()
    {
        var bytes = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        var chars = new char[16];
        for (int i = 0; i < bytes.Length; i++)
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        return new string(chars);
    }
}

public sealed record UserSummary(
    string Id,
    string Email,
    string? DisplayName,
    string Role,
    bool MustChangePassword);
