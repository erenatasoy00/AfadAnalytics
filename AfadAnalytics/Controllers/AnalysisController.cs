using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfadAnalytics.Data;
using AfadAnalytics.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace AfadAnalytics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalysisController(AppDbContext context) { _context = context; }

        [HttpGet("search")]
        public async Task<IActionResult> GetFilteredData(
            [FromQuery] string? city,
            [FromQuery] string? district,
            [FromQuery] string? rooms,
            [FromQuery] string? floor,
            [FromQuery] string? riskCategory,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            try
            {
                var query = _context.PropertyListings.AsQueryable();

                if (!string.IsNullOrWhiteSpace(city))
                    query = query.Where(p => p.City.ToLower().Trim() == city.ToLower().Trim());

                if (!string.IsNullOrWhiteSpace(district))
                    query = query.Where(p => p.District.ToLower().Trim() == district.ToLower().Trim());

                if (!string.IsNullOrWhiteSpace(rooms))
                    query = query.Where(p => p.RoomCount.ToLower().Trim() == rooms.ToLower().Trim());

                if (!string.IsNullOrWhiteSpace(floor))
                    query = query.Where(p => p.FloorLevel.ToLower().Trim() == floor.ToLower().Trim());

                if (!string.IsNullOrWhiteSpace(riskCategory))
                {
                    query = from p in query
                            join r in _context.DistrictRisks on p.District equals r.DistrictName
                            where r.RiskCategory.ToLower().Trim() == riskCategory.ToLower().Trim()
                            select p;
                }

                var allData = await query.ToListAsync();

                if (minPrice.HasValue)
                    allData = allData.Where(p => ParsePrice(p.AskingPriceTry) >= minPrice.Value).ToList();

                if (maxPrice.HasValue)
                    allData = allData.Where(p => ParsePrice(p.AskingPriceTry) <= maxPrice.Value).ToList();

                return ProcessResults(allData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private decimal ParsePrice(string? priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText)) return 0;
            string cleanPrice = new string(priceText.Where(char.IsDigit).ToArray());
            return decimal.TryParse(cleanPrice, out decimal result) ? result : 0;
        }

        private IActionResult ProcessResults(List<PropertyListing> data)
        {
            if (data == null || !data.Any())
                return NotFound(new { message = "Kriterlere uygun ilan bulunamadı." });

            var result = data.Select(p => new {
                id = p.ListingId,
                address = $"{p.City} / {p.District} / {p.Neighborhood}",
                price = p.AskingPriceTry,
                details = new { rooms = p.RoomCount, floor = p.FloorLevel, sqm = p.GrossSqm },
                url = p.Url
            });

            return Ok(result);
        }
    }
}