using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketingPacksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MarketingPacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MarketingPacks
        [HttpGet]
        public async Task<ActionResult<object>> GetMarketingPacks(int page = 1, int pageSize = 10, string? search = null)
        {
            var query = _context.MarketingPacks.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            var total = await query.CountAsync();
            var packs = await query.OrderByDescending(p => p.CreatedAt)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new { data = packs, total };
        }

        // GET: api/MarketingPacks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MarketingPack>> GetMarketingPack(Guid id)
        {
            var marketingPack = await _context.MarketingPacks.FindAsync(id);

            if (marketingPack == null)
            {
                return NotFound();
            }

            return marketingPack;
        }

        // POST: api/MarketingPacks
        [HttpPost]
        public async Task<ActionResult<MarketingPack>> PostMarketingPack(MarketingPack marketingPack)
        {
            marketingPack.Id = Guid.NewGuid();
            marketingPack.CreatedAt = DateTime.UtcNow;
            _context.MarketingPacks.Add(marketingPack);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMarketingPack", new { id = marketingPack.Id }, marketingPack);
        }

        // PUT: api/MarketingPacks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMarketingPack(Guid id, MarketingPack marketingPack)
        {
            if (id != marketingPack.Id)
            {
                return BadRequest();
            }

            _context.Entry(marketingPack).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.MarketingPacks.Any(e => e.Id == id))
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

        // DELETE: api/MarketingPacks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarketingPack(Guid id)
        {
            var marketingPack = await _context.MarketingPacks.FindAsync(id);
            if (marketingPack == null)
            {
                return NotFound();
            }

            _context.MarketingPacks.Remove(marketingPack);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
