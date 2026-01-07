using AutonomousMarketingPlatform.Application.DTOs.Identity;
using AutonomousMarketingPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public RolesController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _identityService.GetRolesAsync();
        return Ok(new { data = roles });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var role = await _identityService.GetRoleByIdAsync(id);
        if (role == null) return NotFound();
        return Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto roleDto)
    {
        if (await _identityService.CreateRoleAsync(roleDto))
            return Ok(new { success = true });
        return BadRequest(new { success = false, message = "Failed to create role" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateRoleDto roleDto)
    {
        if (id != roleDto.Id) return BadRequest();
        if (await _identityService.UpdateRoleAsync(roleDto))
            return Ok(new { success = true });
        return BadRequest(new { success = false, message = "Failed to update role" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (await _identityService.DeleteRoleAsync(id))
            return Ok(new { success = true });
        return BadRequest(new { success = false, message = "Failed to delete role" });
    }
}
