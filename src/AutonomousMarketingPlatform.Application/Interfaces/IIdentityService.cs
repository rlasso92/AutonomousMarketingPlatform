using AutonomousMarketingPlatform.Application.DTOs.Identity;
using AutonomousMarketingPlatform.Domain.Entities;

namespace AutonomousMarketingPlatform.Application.Interfaces;

public interface IIdentityService
{
    // User Methods
    Task<(IEnumerable<UserDto> Users, int TotalCount)> GetUsersAsync(int page, int pageSize, string search);
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<bool> CreateUserAsync(CreateUserDto userDto);
    Task<bool> UpdateUserAsync(UpdateUserDto userDto);
    Task<bool> DeleteUserAsync(string id);

    // Role Methods
    Task<IEnumerable<RoleDto>> GetRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(string id);
    Task<bool> CreateRoleAsync(CreateRoleDto roleDto);
    Task<bool> UpdateRoleAsync(UpdateRoleDto roleDto);
    Task<bool> DeleteRoleAsync(string id);

    // Auth Methods
    Task<bool> LoginAsync(LoginDto loginDto);
    Task LogoutAsync();
}
