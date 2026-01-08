using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConsentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Consents
        [HttpGet]
        public async Task<ActionResult<object>> GetConsents(int page = 1, int pageSize = 10, string? search = null)
        {
            var query = _context.Consents.Include(c => c.Contact).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                // Assuming search will be on the Contact's email
                query = query.Where(c => c.Contact.Email.Contains(search));
            }

            var total = await query.CountAsync();
            var consents = await query.OrderByDescending(c => c.GrantedAt)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .Select(c => new
                                      {
                                          c.Id,
                                          c.ConsentType,
                                          c.Status,
                                          c.GrantedAt,
                                          c.RevokedAt,
                                          c.ContactId,
                                          ContactEmail = c.Contact.Email
                                      })
                                      .ToListAsync();

            return new { data = consents, total };
        }

        // GET: api/Consents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Consent>> GetConsent(Guid id)
        {
            var consent = await _context.Consents.FindAsync(id);

            if (consent == null)
            {
                return NotFound();
            }

            return consent;
        }

        // POST: api/Consents
        [HttpPost]
        public async Task<ActionResult<Consent>> PostConsent(Consent consent)
        {
            // Simple validation to ensure the Contact exists
            var contactExists = await _context.Contacts.AnyAsync(c => c.Id == consent.ContactId);
            if (!contactExists)
            {
                return BadRequest(new { message = "Invalid ContactId" });
            }

            consent.Id = Guid.NewGuid();
            consent.GrantedAt = DateTime.UtcNow;
            _context.Consents.Add(consent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConsent", new { id = consent.Id }, consent);
        }

        // PUT: api/Consents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConsent(Guid id, Consent consent)
        {
            if (id != consent.Id)
            {
                return BadRequest();
            }

            // If status is being changed to Revoked, set the date
            var originalConsent = await _context.Consents.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (originalConsent != null && originalConsent.Status == ConsentStatus.Granted && consent.Status == ConsentStatus.Revoked)
            {
                consent.RevokedAt = DateTime.UtcNow;
            }

            _context.Entry(consent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Consents.Any(e => e.Id == id))
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

        // DELETE: api/Consents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsent(Guid id)
        {
            var consent = await _context.Consents.FindAsync(id);
            if (consent == null)
            {
                return NotFound();
            }

            _context.Consents.Remove(consent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
