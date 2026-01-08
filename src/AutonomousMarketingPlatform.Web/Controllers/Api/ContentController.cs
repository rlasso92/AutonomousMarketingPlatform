using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Content
        [HttpGet]
        public async Task<ActionResult<object>> GetContent(int page = 1, int pageSize = 10, string? search = null)
        {
            var query = _context.Content.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }

            var total = await query.CountAsync();
            var content = await query.OrderByDescending(c => c.CreatedAt)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();

            return new { data = content, total };
        }

        // GET: api/Content/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Content>> GetContentItem(Guid id)
        {
            var content = await _context.Content.FindAsync(id);

            if (content == null)
            {
                return NotFound();
            }

            return content;
        }

        // POST: api/Content
        [HttpPost]
        public async Task<ActionResult<Content>> PostContent(Content content)
        {
            content.Id = Guid.NewGuid();
            content.CreatedAt = DateTime.UtcNow;
            _context.Content.Add(content);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContentItem", new { id = content.Id }, content);
        }

        // PUT: api/Content/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContent(Guid id, Content content)
        {
            if (id != content.Id)
            {
                return BadRequest();
            }

            _context.Entry(content).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Content.Any(e => e.Id == id))
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

        // DELETE: api/Content/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContent(Guid id)
        {
            var content = await _context.Content.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }

            _context.Content.Remove(content);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
