using AutonomousMarketingPlatform.Application.DTOs.Identity;
using AutonomousMarketingPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Invalid data" });
        }

        var result = await _identityService.LoginAsync(loginDto);
        if (result)
        {
            return Ok(new { success = true });
        }
        return Unauthorized(new { success = false, message = "Invalid email or password" });
    }
}
