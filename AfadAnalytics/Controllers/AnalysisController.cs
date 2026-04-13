using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfadAnalytics.Data;
using AfadAnalytics.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using AfadAnalytics.DTOs;

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
                {
                    string searchCity = $"%{city.Trim()}%";
                    query = query.Where(p => EF.Functions.ILike(p.City, searchCity));
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    string searchDistrict = $"%{district.Trim()}%";
                    query = query.Where(p => EF.Functions.ILike(p.District, searchDistrict));
                }

                if (!string.IsNullOrWhiteSpace(rooms))
                {
                    string searchRooms = $"%{rooms.Trim()}%";
                    query = query.Where(p => EF.Functions.ILike(p.RoomCount, searchRooms));
                }

                if (!string.IsNullOrWhiteSpace(floor))
                {
                    string searchFloor = $"%{floor.Trim()}%";
                    query = query.Where(p => EF.Functions.ILike(p.FloorLevel, searchFloor));
                }

                if (!string.IsNullOrWhiteSpace(riskCategory))
                {
                    string searchRisk = $"%{riskCategory.Trim()}%";
                    query = from p in query
                            join r in _context.DistrictRisks on p.District equals r.DistrictName
                            where EF.Functions.ILike(r.RiskCategory, searchRisk)
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

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary(
            [FromQuery] string? city,
            [FromQuery] string? district,
            [FromQuery] string? riskCategory,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            try
            {
                var query = from p in _context.PropertyListings
                            join r in _context.DistrictRisks on p.District equals r.DistrictName
                            select new { Property = p, RiskCategory = r.RiskCategory };

                if (!string.IsNullOrWhiteSpace(city))
                {
                    string searchCity = $"%{city.Trim()}%";
                    query = query.Where(q => EF.Functions.ILike(q.Property.City, searchCity));
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    string searchDistrict = $"%{district.Trim()}%";
                    query = query.Where(q => EF.Functions.ILike(q.Property.District, searchDistrict));
                }

                if (!string.IsNullOrWhiteSpace(riskCategory))
                {
                    string searchRisk = $"%{riskCategory.Trim()}%";
                    query = query.Where(q => q.RiskCategory != null && EF.Functions.ILike(q.RiskCategory, searchRisk));
                }

                var allData = await query.ToListAsync();

                if (minPrice.HasValue)
                    allData = allData.Where(q => ParsePrice(q.Property.AskingPriceTry) >= minPrice.Value).ToList();

                if (maxPrice.HasValue)
                    allData = allData.Where(q => ParsePrice(q.Property.AskingPriceTry) <= maxPrice.Value).ToList();

                var totalListings = allData.Count;

                if (totalListings == 0)
                {
                    return Ok(new SummaryMetricsDTO
                    {
                        TotalListings = 0,
                        AveragePrice = 0,
                        HighRiskCount = 0,
                        LowRiskCount = 0
                    });
                }

                var averagePrice = allData.Average(q => ParsePrice(q.Property.AskingPriceTry));

               
                var highRiskCount = allData.Count(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains("yüksek"));
                var lowRiskCount = allData.Count(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains("düşük"));

                var result = new SummaryMetricsDTO
                {
                    TotalListings = totalListings,
                    AveragePrice = Math.Round(averagePrice, 2),
                    HighRiskCount = highRiskCount,
                    LowRiskCount = lowRiskCount
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("map-data")]
        public async Task<IActionResult> GetMapData(
            [FromQuery] string? city,
            [FromQuery] string? district,
            [FromQuery] string? riskCategory,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            try
            {
                var query = from p in _context.PropertyListings
                            join r in _context.DistrictRisks on p.District equals r.DistrictName
                            select new
                            {
                                p.ListingId,
                                p.City,
                                p.District,
                                p.AskingPriceTry,
                                p.Latitude,
                                p.Longitude,
                                RiskCategory = r.RiskCategory
                            };

                if (!string.IsNullOrWhiteSpace(city))
                {
                    string searchCity = $"%{city.Trim()}%";
                    query = query.Where(q => EF.Functions.ILike(q.City, searchCity));
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    string searchDistrict = $"%{district.Trim()}%";
                    query = query.Where(q => EF.Functions.ILike(q.District, searchDistrict));
                }

                if (!string.IsNullOrWhiteSpace(riskCategory))
                {
                    string searchRisk = $"%{riskCategory.Trim()}%";
                    query = query.Where(q => q.RiskCategory != null && EF.Functions.ILike(q.RiskCategory, searchRisk));
                }

                var allData = await query.ToListAsync();

                if (minPrice.HasValue)
                    allData = allData.Where(q => ParsePrice(q.AskingPriceTry) >= minPrice.Value).ToList();

                if (maxPrice.HasValue)
                    allData = allData.Where(q => ParsePrice(q.AskingPriceTry) <= maxPrice.Value).ToList();

                var result = allData.Select(q => new MapDataDTO
                {
                    ListingId = q.ListingId,
                    Price = ParsePrice(q.AskingPriceTry),
                    RiskCategory = q.RiskCategory ?? "Unknown",
                    Latitude = Convert.ToDouble(q.Latitude),
                    Longitude = Convert.ToDouble(q.Longitude)
                }).ToList();

                return Ok(result);
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
                return NotFound(new { message = "No matching listings found." });

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