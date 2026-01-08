using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishJobsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublishJobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PublishJobs
        [HttpGet]
        public async Task<ActionResult<object>> GetPublishJobs(int page = 1, int pageSize = 10, string? search = null)
        {
            var query = _context.PublishJobs.Include(j => j.Content).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(j => j.Channel.Contains(search) || j.Content.Title.Contains(search));
            }

            var total = await query.CountAsync();
            var jobs = await query.OrderByDescending(j => j.ScheduledTime)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(j => new
                                  {
                                      j.Id,
                                      j.Channel,
                                      j.ScheduledTime,
                                      j.Status,
                                      j.ContentId,
                                      ContentTitle = j.Content.Title
                                  })
                                  .ToListAsync();

            return new { data = jobs, total };
        }

        // GET: api/PublishJobs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PublishJob>> GetPublishJob(Guid id)
        {
            var publishJob = await _context.PublishJobs.FindAsync(id);

            if (publishJob == null)
            {
                return NotFound();
            }

            return publishJob;
        }

        // POST: api/PublishJobs
        [HttpPost]
        public async Task<ActionResult<PublishJob>> PostPublishJob(PublishJob publishJob)
        {
            publishJob.Id = Guid.NewGuid();
            publishJob.CreatedAt = DateTime.UtcNow;
            _context.PublishJobs.Add(publishJob);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPublishJob", new { id = publishJob.Id }, publishJob);
        }

        // PUT: api/PublishJobs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPublishJob(Guid id, PublishJob publishJob)
        {
            if (id != publishJob.Id)
            {
                return BadRequest();
            }

            _context.Entry(publishJob).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PublishJobs.Any(e => e.Id == id))
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

        // DELETE: api/PublishJobs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePublishJob(Guid id)
        {
            var publishJob = await _context.PublishJobs.FindAsync(id);
            if (publishJob == null)
            {
                return NotFound();
            }

            _context.PublishJobs.Remove(publishJob);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
