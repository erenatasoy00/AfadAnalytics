using Microsoft.AspNetCore.Mvc;
using AfadAnalytics.Services.MapService;

namespace AfadAnalytics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly IMapService _mapService;

        public MapController(IMapService mapService)
        {
            _mapService = mapService;
        }

        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] string? city)
        {
            var result = await _mapService.GetDistrictsAsync(city);
            return Ok(result);
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces([FromQuery] string scenario = "S1_Balanced")
        {
            var result = await _mapService.GetProvincesAsync(scenario);
            if (result == null || !result.Any())
                return NotFound(new { message = $"No data found for scenario: {scenario}" });
            return Ok(result);
        }

        [HttpGet("province/{name}")]
        public async Task<IActionResult> GetProvinceDetail(string name, [FromQuery] string scenario = "S1_Balanced")
        {
            var result = await _mapService.GetProvinceDetailAsync(name, scenario);
            if (result == null)
                return NotFound(new { message = $"{name} not found for scenario: {scenario}" });
            return Ok(result);
        }
    }
}