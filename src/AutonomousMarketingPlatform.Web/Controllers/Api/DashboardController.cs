using AutonomousMarketingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("metrics-summary")]
        public async Task<IActionResult> GetMetricsSummary()
        {
            var totalEmailsSent = await _context.CampaignMetrics.SumAsync(m => m.EmailsSent);
            var totalEmailsOpened = await _context.CampaignMetrics.SumAsync(m => m.EmailsOpened);
            var totalClicks = await _context.CampaignMetrics.SumAsync(m => m.Clicks);
            var totalSocialImpressions = await _context.CampaignMetrics.SumAsync(m => m.Impressions);

            return Ok(new
            {
                totalEmailsSent,
                totalEmailsOpened,
                totalClicks,
                totalSocialImpressions
            });
        }

        [HttpGet("email-performance-chart-data")]
        public async Task<IActionResult> GetEmailPerformanceChartData()
        {
            // Group by date (e.g., daily) and sum metrics
            var data = await _context.CampaignMetrics
                .GroupBy(m => m.LastUpdated.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    EmailsSent = g.Sum(m => m.EmailsSent),
                    EmailsOpened = g.Sum(m => m.EmailsOpened),
                    Clicks = g.Sum(m => m.Clicks)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("social-engagement-chart-data")]
        public async Task<IActionResult> GetSocialEngagementChartData()
        {
            // Group by campaign or a time period, here by campaign for simplicity
            var data = await _context.CampaignMetrics
                .GroupBy(m => m.Campaign.Name) // Assuming Campaign.Name is a good grouping for social engagement
                .Select(g => new
                {
                    CampaignName = g.Key,
                    Impressions = g.Sum(m => m.Impressions),
                    Likes = g.Sum(m => m.Likes),
                    Shares = g.Sum(m => m.Shares),
                    Comments = g.Sum(m => m.Comments)
                })
                .OrderByDescending(x => x.Impressions)
                .Take(5) // Top 5 campaigns by impressions
                .ToListAsync();

            return Ok(data);
        }
    }
}
