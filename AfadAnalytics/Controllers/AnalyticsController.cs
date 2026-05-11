using Microsoft.AspNetCore.Mvc;
using AfadAnalytics.Services.AnalyticsService;

namespace AfadAnalytics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("price-by-city")]
        public async Task<IActionResult> GetPriceByCity(
            [FromQuery] string? city,
            [FromQuery] string? riskCategory,
            [FromQuery] string? listingType)
        {
            var result = await _analyticsService.GetPriceByCityAsync(city, riskCategory, listingType);
            return Ok(result);
        }

        [HttpGet("price-by-risk")]
        public async Task<IActionResult> GetPriceByRisk(
            [FromQuery] string? city,
            [FromQuery] string? listingType)
        {
            var result = await _analyticsService.GetPriceByRiskAsync(city, listingType);
            return Ok(result);
        }

        [HttpGet("scatter")]
        public async Task<IActionResult> GetScatterData(
            [FromQuery] string? city)
        {
            var result = await _analyticsService.GetScatterDataAsync(city);
            return Ok(result);
        }

        [HttpGet("top-districts")]
        public async Task<IActionResult> GetTopDistricts(
            [FromQuery] string? city,
            [FromQuery] string? riskCategory,
            [FromQuery] int top = 4)
        {
            var result = await _analyticsService.GetTopDistrictsAsync(city, riskCategory, top);
            return Ok(result);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(
            [FromQuery] string? city,
            [FromQuery] string? riskCategory,
            [FromQuery] string? listingType)
        {
            var result = await _analyticsService.GetStatsAsync(city, riskCategory, listingType);
            return Ok(result);
        }

        [HttpGet("city-comparison")]
        public async Task<IActionResult> GetCityComparison()
        {
            var result = await _analyticsService.GetCityComparisonAsync();
            return Ok(result);
        }
    }
}