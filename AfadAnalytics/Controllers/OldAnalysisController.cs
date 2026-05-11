
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfadAnalytics.Data;
using AfadAnalytics.Models;
using AfadAnalytics.DTOs;
using System.Globalization;

namespace AfadAnalytics.Controllers
{
    /*
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalysisController(AppDbContext context) { _context = context; }

        /// <summary>
        /// Retrieves a filtered list of property listings based on specified criteria.
        /// </summary>
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
                    string searchCity = city.Trim().ToLower();
                    query = query.Where(p => p.City != null && p.City.ToLower().Contains(searchCity));
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    string searchDistrict = district.Trim().ToLower();
                    query = query.Where(p => p.District != null && p.District.ToLower().Contains(searchDistrict));
                }

                if (!string.IsNullOrWhiteSpace(rooms))
                {
                    string searchRooms = rooms.Trim().ToLower();
                    query = query.Where(p => p.RoomCount != null && p.RoomCount.ToLower().Contains(searchRooms));
                }

                if (!string.IsNullOrWhiteSpace(floor))
                {
                    string searchFloor = floor.Trim().ToLower();
                    query = query.Where(p => p.FloorLevel != null && p.FloorLevel.ToLower().Contains(searchFloor));
                }

                if (!string.IsNullOrWhiteSpace(riskCategory))
                {
                    string searchRisk = riskCategory.Trim().ToLower();
                    query = from p in query
                            join r in _context.DistrictRisks on p.District equals r.DistrictName
                            where r.RiskCategory != null && r.RiskCategory.ToLower().Contains(searchRisk)
                            select p;
                }

                var allData = await query.ToListAsync();

                // Hata Çözümü: AskingPriceTry artık double? olduğu için doğrudan karşılaştırma yapıyoruz
                if (minPrice.HasValue)
                    allData = allData.Where(p => (decimal?)(p.AskingPriceTry ?? 0) >= minPrice.Value).ToList();

                if (maxPrice.HasValue)
                    allData = allData.Where(p => (decimal?)(p.AskingPriceTry ?? 0) <= maxPrice.Value).ToList();

                return ProcessResults(allData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Calculates summary statistics for the top dashboard panel.
        /// </summary>
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
                    string searchCity = city.Trim().ToLower();
                    query = query.Where(q => q.Property.City != null && q.Property.City.ToLower().Contains(searchCity));
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    string searchDistrict = district.Trim().ToLower();
                    query = query.Where(q => q.Property.District != null && q.Property.District.ToLower().Contains(searchDistrict));
                }

                if (!string.IsNullOrWhiteSpace(riskCategory))
                {
                    string searchRisk = riskCategory.Trim().ToLower();
                    query = query.Where(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains(searchRisk));
                }

                var allData = await query.ToListAsync();

                if (minPrice.HasValue)
                    allData = allData.Where(q => (decimal?)(q.Property.AskingPriceTry ?? 0) >= minPrice.Value).ToList();

                if (maxPrice.HasValue)
                    allData = allData.Where(q => (decimal?)(q.Property.AskingPriceTry ?? 0) <= maxPrice.Value).ToList();

                var totalListings = allData.Count;
                if (totalListings == 0)
                {
                    return Ok(new SummaryMetricsDTO { TotalListings = 0, AveragePrice = 0, HighRiskCount = 0, LowRiskCount = 0 });
                }

                // Ortalama hesaplama sayısal veri ile çok daha kolay
                var averagePrice = allData.Average(q => q.Property.AskingPriceTry ?? 0);
                var highRiskCount = allData.Count(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains("yüksek"));
                var lowRiskCount = allData.Count(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains("düşük"));

                return Ok(new SummaryMetricsDTO
                {
                    TotalListings = totalListings,
                    AveragePrice = Math.Round((decimal)averagePrice, 2),
                    HighRiskCount = highRiskCount,
                    LowRiskCount = lowRiskCount
                });
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }

        [HttpGet("district-summary")]
        public async Task<IActionResult> GetDistrictSummaries([FromQuery] string? city)
        {
            try
            {
                var sql = @"
                    SELECT p.city, p.district, p.latitude, p.longitude,
                           p.asking_price_try, p.price_per_sqm_try,
                           r.risk_category, r.composite_risk_score as risk_score
                    FROM property_listings p
                    JOIN district_risks r 
                        ON unaccent(lower(p.district)) = unaccent(lower(r.district))";

                if (!string.IsNullOrWhiteSpace(city))
                    sql += $" AND lower(p.city) LIKE lower('%{city.Trim()}%')";

                // Not: DistrictSummaryRaw modelindeki tiplerin de double/decimal olduğundan emin olmalısın
                var allData = await _context.Database
                    .SqlQueryRaw<DistrictSummaryRaw>(sql)
                    .ToListAsync();

                var groupedData = allData
                    .GroupBy(x => new { x.City, x.District })
                    .Select(g => new
                    {
                        city = g.Key.City,
                        district = g.Key.District,
                        lat = g.First().Latitude ?? 39.0,
                        lng = g.First().Longitude ?? 35.0,
                        risk_score = g.First().RiskScore,
                        risk_category = g.First().RiskCategory ?? "Bilinmiyor",
                        listing_count = g.Count(),
                        // AskingPriceTry artık double olduğu için ParsePrice'a gerek kalmadan direkt ortalama alıyoruz
                        avg_sale_price = Math.Round((decimal)(g.Average(x => x.AskingPriceTry) ?? 0), 2),

                        // PricePerSqmTry SQL'de hala TEXT (string) olduğu için ParsePrice ile sayıya çeviriyoruz
                        avg_price_per_m2 = g.Any(x => !string.IsNullOrWhiteSpace(x.PricePerSqmTry))
                            ? Math.Round((decimal)g.Where(x => !string.IsNullOrWhiteSpace(x.PricePerSqmTry))
                                .Average(x => ParsePrice(x.PricePerSqmTry)), 2)
                            : 0
                    }).ToList();
                return Ok(groupedData);
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }

        // provinces ve province/{name} endpoint'leri modeller düzgünse hata vermez, 
        // ancak onlarda da ParsePrice kullanımı varsa silinmelidir.

        private decimal ParsePrice(string? priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText)) return 0;
            string cleanPrice = priceText.Replace("TL", "").Replace(" ", "").Trim();
            if (cleanPrice.Contains(",") && cleanPrice.Contains(".")) cleanPrice = cleanPrice.Replace(".", "").Replace(",", ".");
            else if (cleanPrice.Contains(",")) cleanPrice = cleanPrice.Replace(",", ".");
            if (decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result)) return result;
            return 0;
        }

        private IActionResult ProcessResults(List<PropertyListing> data)
        {
            if (data == null || !data.Any()) return NotFound(new { message = "No matching listings found." });
            return Ok(data.Select(p => new {
                id = p.ListingId,
                address = $"{p.City} / {p.District}",
                price = p.AskingPriceTry, // Artık doğrudan sayı gidiyor, Frontend'de TL eklemek daha kolaydır
                details = new { rooms = p.RoomCount, floor = p.FloorLevel, sqm = p.GrossSqm },
               
            }));
        }
    }
    */
}
