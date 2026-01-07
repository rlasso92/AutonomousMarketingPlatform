using AutonomousMarketingPlatform.Application.DTOs.Identity;
using AutonomousMarketingPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public UsersController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10, string search = "")
    {
        var (users, totalCount) = await _identityService.GetUsersAsync(page, pageSize, search);
        return Ok(new { data = users, total = totalCount, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _identityService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto userDto)
    {
        if (await _identityService.CreateUserAsync(userDto))
            return Ok(new { success = true });
        return BadRequest(new { success = false, message = "Failed to create user" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto userDto)
    {
        if (id != userDto.Id) return BadRequest();
        if (await _identityService.UpdateUserAsync(userDto))
            return Ok(new { success = true });
        return BadRequest(new { success = false, message = "Failed to update user" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (await _identityService.DeleteUserAsync(id))
            return Ok(new { success = true });
        return BadRequest(new { success = false, message = "Failed to delete user" });
    }
}
