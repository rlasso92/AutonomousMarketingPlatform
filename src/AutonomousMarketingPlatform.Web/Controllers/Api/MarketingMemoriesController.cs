using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketingMemoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MarketingMemoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MarketingMemories
        [HttpGet]
        public async Task<ActionResult<object>> GetMarketingMemories(int page = 1, int pageSize = 10, string? search = null)
        {
            var query = _context.MarketingMemories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Key.Contains(search) || m.Value.Contains(search));
            }

            var total = await query.CountAsync();
            var memories = await query.OrderByDescending(m => m.LastAccessed)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            return new { data = memories, total };
        }

        // GET: api/MarketingMemories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MarketingMemory>> GetMarketingMemory(Guid id)
        {
            var memory = await _context.MarketingMemories.FindAsync(id);

            if (memory == null)
            {
                return NotFound();
            }

            // Update LastAccessed timestamp
            memory.LastAccessed = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return memory;
        }

        // POST: api/MarketingMemories
        [HttpPost]
        public async Task<ActionResult<MarketingMemory>> PostMarketingMemory(MarketingMemory memory)
        {
            memory.Id = Guid.NewGuid();
            memory.LastAccessed = DateTime.UtcNow;
            _context.MarketingMemories.Add(memory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMarketingMemory", new { id = memory.Id }, memory);
        }

        // PUT: api/MarketingMemories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMarketingMemory(Guid id, MarketingMemory memory)
        {
            if (id != memory.Id)
            {
                return BadRequest();
            }

            memory.LastAccessed = DateTime.UtcNow;
            _context.Entry(memory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.MarketingMemories.Any(e => e.Id == id))
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

        // DELETE: api/MarketingMemories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarketingMemory(Guid id)
        {
            var memory = await _context.MarketingMemories.FindAsync(id);
            if (memory == null)
            {
                return NotFound();
            }

            _context.MarketingMemories.Remove(memory);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
