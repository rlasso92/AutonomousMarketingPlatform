using AutonomousMarketingPlatform.Application.DTOs.Identity;
using AutonomousMarketingPlatform.Application.Interfaces;
using AutonomousMarketingPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }

    // Auth Implementation
    public async Task<bool> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) return false;

        var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: false);
        return result.Succeeded;
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    // User Implementation
    public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetUsersAsync(int page, int pageSize, string search)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                (u.UserName != null && u.UserName.Contains(search)) ||
                (u.Email != null && u.Email.Contains(search)) ||
                (u.FirstName != null && u.FirstName.Contains(search)) ||
                (u.LastName != null && u.LastName.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Roles = roles
            });
        }

        return (userDtos, totalCount);
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Roles = roles
        };
    }

    public async Task<bool> CreateUserAsync(CreateUserDto userDto)
    {
        var user = new ApplicationUser
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            PhoneNumber = userDto.PhoneNumber,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, userDto.Password);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(userDto.Role))
            {
                if (await _roleManager.RoleExistsAsync(userDto.Role))
                {
                    await _userManager.AddToRoleAsync(user, userDto.Role);
                }
            }
            return true;
        }
        return false;
    }

    public async Task<bool> UpdateUserAsync(UpdateUserDto userDto)
    {
        var user = await _userManager.FindByIdAsync(userDto.Id);
        if (user == null) return false;

        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.Email = userDto.Email;
        user.UserName = userDto.Email; // Keep username synced with email
        user.PhoneNumber = userDto.PhoneNumber ?? user.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!string.IsNullOrEmpty(userDto.Role) && await _roleManager.RoleExistsAsync(userDto.Role))
        {
            await _userManager.AddToRoleAsync(user, userDto.Role);
        }

        return true;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    // Role Implementation
    public async Task<IEnumerable<RoleDto>> GetRolesAsync()
    {
        return await _roleManager.Roles
            .Select(r => new RoleDto { Id = r.Id, Name = r.Name })
            .ToListAsync();
    }

    public async Task<RoleDto?> GetRoleByIdAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return null;
        return new RoleDto { Id = role.Id, Name = role.Name ?? string.Empty };
    }

    public async Task<bool> CreateRoleAsync(CreateRoleDto roleDto)
    {
        if (await _roleManager.RoleExistsAsync(roleDto.Name)) return false;
        var result = await _roleManager.CreateAsync(new ApplicationRole(roleDto.Name));
        return result.Succeeded;
    }

    public async Task<bool> UpdateRoleAsync(UpdateRoleDto roleDto)
    {
        var role = await _roleManager.FindByIdAsync(roleDto.Id);
        if (role == null) return false;
        if (string.IsNullOrEmpty(roleDto.Name)) return false;
        role.Name = roleDto.Name!;
        var result = await _roleManager.UpdateAsync(role);
        return result.Succeeded;
    }

    public async Task<bool> DeleteRoleAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return false;
        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded;
    }
}
