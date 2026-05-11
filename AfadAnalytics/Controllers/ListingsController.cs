using Microsoft.AspNetCore.Mvc;
using AfadAnalytics.Services.ListingService;

namespace AfadAnalytics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsController : ControllerBase
    {
        private readonly IListingsService _listingsService;

        public ListingsController(IListingsService listingsService)
        {
            _listingsService = listingsService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? city,
            [FromQuery] string? district,
            [FromQuery] string? rooms,
            [FromQuery] string? floor,
            [FromQuery] string? riskCategory,
            [FromQuery] string? listingType,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var result = await _listingsService.SearchAsync(
                city, district, rooms, floor, riskCategory, listingType, minPrice, maxPrice);

            if (result == null || !result.Any())
                return NotFound(new { message = "No matching listings found." });

            return Ok(result);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] string? city,
            [FromQuery] string? district,
            [FromQuery] string? riskCategory,
            [FromQuery] string? listingType,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var result = await _listingsService.GetDashboardSummaryAsync(
                city, district, riskCategory, listingType, minPrice, maxPrice);

            return Ok(result);
        }
    }
}