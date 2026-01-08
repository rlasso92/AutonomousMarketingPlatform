using System;
using System.Threading.Tasks;
using AutonomousMarketingPlatform.Application.DTOs.Identity;
using AutonomousMarketingPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.Organization,
                user.Department,
                user.Location,
                user.Bio,
                user.AvatarBase64
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileDto profileDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Organization = profileDto.Organization;
            user.Department = profileDto.Department;
            user.Location = profileDto.Location;
            user.Bio = profileDto.Bio;
            user.SocialMediaLinks = profileDto.SocialMediaLinks;

            if (!string.IsNullOrEmpty(profileDto.AvatarBase64))
            {
                // The Base64 string from JS might include a prefix (e.g., "data:image/png;base64,"), remove it.
                var base64Data = profileDto.AvatarBase64.Split(',').Last();
                user.AvatarBase64 = base64Data;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto passwordDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, passwordDto.CurrentPassword, passwordDto.NewPassword);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("setup-2fa")]
        public async Task<IActionResult> SetupTwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var secretKey = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            user.TwoFactorSecretKey = secretKey;
            await _userManager.UpdateAsync(user);

            var twoFactorAuthenticator = new Google.Authenticator.TwoFactorAuthenticator();
            var setupInfo = twoFactorAuthenticator.GenerateSetupCode("AutonomousMarketingPlatform", user.Email, secretKey, false, 3);

            return Ok(new { QrCodeImageUrl = setupInfo.QrCodeSetupImageUrl, ManualEntryKey = setupInfo.ManualEntryKey });
        }

        [HttpPost("enable-2fa")]
        public async Task<IActionResult> EnableTwoFactorAuthentication([FromBody] Enable2faDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecretKey)) return BadRequest("2FA not set up or user not found.");

            var twoFactorAuthenticator = new Google.Authenticator.TwoFactorAuthenticator();
            bool isValid = twoFactorAuthenticator.ValidateTwoFactorPIN(user.TwoFactorSecretKey, dto.Code);

            if (!isValid)
            {
                return BadRequest("Invalid code.");
            }

            user.TwoFactorEnabled = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }
    }

    public class UpdateProfileDto
    {
        public string? Organization { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? Bio { get; set; }
        public string? AvatarBase64 { get; set; }
        public string? SocialMediaLinks { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class Enable2faDto
    {
        public string Code { get; set; } = string.Empty;
    }
}
