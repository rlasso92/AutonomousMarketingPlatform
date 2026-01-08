using System.Text.Json;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Infrastructure.Persistence;
using AutonomousMarketingPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public IntegrationSettingsController(ApplicationDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        // GET: api/IntegrationSettings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IntegrationSettingDto>>> GetIntegrationSettings()
        {
            var settings = await _context.IntegrationSettings.ToListAsync();
            var dtos = settings.Select(s => new IntegrationSettingDto
            {
                Id = s.Id,
                Category = s.Category,
                Service = s.Service,
                DisplayName = s.DisplayName,
                IconClass = s.IconClass,
                IsEnabled = s.IsEnabled,
                // Decrypt config for display/edit, but be careful with exposing sensitive data
                ConfigJson = _encryptionService.Decrypt(s.EncryptedConfigJson)
            });
            return Ok(dtos);
        }

        // GET: api/IntegrationSettings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IntegrationSettingDto>> GetIntegrationSetting(Guid id)
        {
            var setting = await _context.IntegrationSettings.FindAsync(id);

            if (setting == null)
            {
                return NotFound();
            }

            return Ok(new IntegrationSettingDto
            {
                Id = setting.Id,
                Category = setting.Category,
                Service = setting.Service,
                DisplayName = setting.DisplayName,
                IconClass = setting.IconClass,
                IsEnabled = setting.IsEnabled,
                ConfigJson = _encryptionService.Decrypt(setting.EncryptedConfigJson)
            });
        }

        // POST: api/IntegrationSettings
        [HttpPost]
        public async Task<ActionResult<IntegrationSettingDto>> PostIntegrationSetting(IntegrationSettingDto dto)
        {
            var setting = new IntegrationSetting
            {
                Id = Guid.NewGuid(),
                Category = dto.Category,
                Service = dto.Service,
                DisplayName = dto.DisplayName,
                IconClass = dto.IconClass,
                IsEnabled = dto.IsEnabled,
                EncryptedConfigJson = _encryptionService.Encrypt(dto.ConfigJson)
            };

            _context.IntegrationSettings.Add(setting);
            await _context.SaveChangesAsync();

            dto.Id = setting.Id; // Return with generated ID
            return CreatedAtAction("GetIntegrationSetting", new { id = setting.Id }, dto);
        }

        // PUT: api/IntegrationSettings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIntegrationSetting(Guid id, IntegrationSettingDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            var setting = await _context.IntegrationSettings.FindAsync(id);
            if (setting == null)
            {
                return NotFound();
            }

            setting.Category = dto.Category;
            setting.Service = dto.Service;
            setting.DisplayName = dto.DisplayName;
            setting.IconClass = dto.IconClass;
            setting.IsEnabled = dto.IsEnabled;
            setting.EncryptedConfigJson = _encryptionService.Encrypt(dto.ConfigJson);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.IntegrationSettings.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/IntegrationSettings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIntegrationSetting(Guid id)
        {
            var setting = await _context.IntegrationSettings.FindAsync(id);
            if (setting == null)
            {
                return NotFound();
            }

            _context.IntegrationSettings.Remove(setting);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO for transferring integration settings data (including decrypted config)
    public class IntegrationSettingDto
    {
        public Guid Id { get; set; }
        public ServiceCategory Category { get; set; }
        public ServiceName Service { get; set; }
        public string DisplayName { get; set; } = null!;
        public string IconClass { get; set; } = null!;
        public bool IsEnabled { get; set; }
        public string ConfigJson { get; set; } = null!;
    }
}
