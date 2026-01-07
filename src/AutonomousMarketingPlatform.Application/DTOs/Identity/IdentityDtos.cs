namespace AutonomousMarketingPlatform.Application.DTOs.Identity;

public class UserDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public IList<string> Roles { get; set; }
}

public class CreateUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
}

public class UpdateUserDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
}

public class RoleDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class CreateRoleDto
{
    public string Name { get; set; }
}

public class UpdateRoleDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}
