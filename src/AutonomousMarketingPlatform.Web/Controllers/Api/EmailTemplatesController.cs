using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmailTemplatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/EmailTemplates
        [HttpGet]
        public async Task<ActionResult<object>> GetEmailTemplates(int page = 1, int pageSize = 10, string? search = null)
        {
            var query = _context.EmailTemplates.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.Contains(search) || t.Subject.Contains(search));
            }

            var total = await query.CountAsync();
            var templates = await query.OrderByDescending(t => t.UpdatedAt)
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

            return new { data = templates, total };
        }

        // GET: api/EmailTemplates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmailTemplate>> GetEmailTemplate(Guid id)
        {
            var emailTemplate = await _context.EmailTemplates.FindAsync(id);

            if (emailTemplate == null)
            {
                return NotFound();
            }

            return emailTemplate;
        }

        // POST: api/EmailTemplates
        [HttpPost]
        public async Task<ActionResult<EmailTemplate>> PostEmailTemplate(EmailTemplate emailTemplate)
        {
            emailTemplate.Id = Guid.NewGuid();
            emailTemplate.CreatedAt = DateTime.UtcNow;
            emailTemplate.UpdatedAt = DateTime.UtcNow;
            _context.EmailTemplates.Add(emailTemplate);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmailTemplate", new { id = emailTemplate.Id }, emailTemplate);
        }

        // PUT: api/EmailTemplates/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmailTemplate(Guid id, EmailTemplate emailTemplate)
        {
            if (id != emailTemplate.Id)
            {
                return BadRequest();
            }

            emailTemplate.UpdatedAt = DateTime.UtcNow;
            _context.Entry(emailTemplate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.EmailTemplates.Any(e => e.Id == id))
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

        // DELETE: api/EmailTemplates/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmailTemplate(Guid id)
        {
            var emailTemplate = await _context.EmailTemplates.FindAsync(id);
            if (emailTemplate == null)
            {
                return NotFound();
            }

            _context.EmailTemplates.Remove(emailTemplate);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
