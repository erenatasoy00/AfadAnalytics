using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfadAnalytics.Data;
using AfadAnalytics.Models;
using AfadAnalytics.DTOs;
using System.Globalization;

namespace AfadAnalytics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalysisController(AppDbContext context) { _context = context; }

        // 1. Arama Endpoint'i
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

        // 2. Dashboard Özet Endpoint'i (Fiyat düzeltmesi eklendi)
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
                    return Ok(new SummaryMetricsDTO { TotalListings = 0, AveragePrice = 0, HighRiskCount = 0, LowRiskCount = 0 });
                }

                // Fiyatlar ParsePrice ile doğru çekildiği için ortalama artık gerçekçi çıkacak
                var averagePrice = allData.Average(q => ParsePrice(q.Property.AskingPriceTry));

                var highRiskCount = allData.Count(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains("yüksek"));
                var lowRiskCount = allData.Count(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains("düşük"));

                return Ok(new SummaryMetricsDTO
                {
                    TotalListings = totalListings,
                    AveragePrice = Math.Round(averagePrice, 2),
                    HighRiskCount = highRiskCount,
                    LowRiskCount = lowRiskCount
                });
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }

        // 3. Harita ve İlçe Özeti Endpoint'i (Koordinat ve Fiyat düzeltmesi eklendi)
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

                var allData = await _context.Database
                    .SqlQueryRaw<DistrictSummaryRaw>(sql)
                    .ToListAsync();

                var groupedData = allData
                    .GroupBy(x => new { x.City, x.District })
                    .Select(g => new
                    {
                        city = g.Key.City,
                        district = g.Key.District,
                        lat = string.IsNullOrEmpty(g.First().Latitude) ? 39.0 : ParseCoordinate(g.First().Latitude),
                        lng = string.IsNullOrEmpty(g.First().Longitude) ? 35.0 : ParseCoordinate(g.First().Longitude),
                        risk_score = g.First().RiskScore,
                        risk_category = g.First().RiskCategory ?? "Bilinmiyor",
                        listing_count = g.Count(),
                        avg_sale_price = Math.Round(g.Average(x => ParsePrice(x.AskingPriceTry)), 2),
                        avg_price_per_m2 = g.Any(x => !string.IsNullOrEmpty(x.PricePerSqm))
                            ? Math.Round(g.Where(x => !string.IsNullOrEmpty(x.PricePerSqm))
                                .Average(x => ParsePrice(x.PricePerSqm)), 2)
                            : 0
                    }).ToList();

                return Ok(groupedData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        // 4. Tüm İller — Senaryo Bazında ERI Skorları
        [HttpGet("provinces")]
        public async Task<IActionResult> GetAllProvinces([FromQuery] string scenario = "S1_Balanced")
        {
            try
            {
                var results = await (
                    from e in _context.EriScores
                    join m in _context.ProvinceMetadata on e.Province equals m.Province
                    where e.ScenarioId == scenario
                    select new
                    {
                        province = e.Province,
                        latitude = m.Latitude,
                        longitude = m.Longitude,
                        afad_zone = m.AfadZone,
                        scenario_id = e.ScenarioId,
                        scenario_label = e.ScenarioLabel,
                        hazard_score = e.HazardScore,
                        vulnerability_score = e.VulnerabilityScore,
                        eri_normalized = e.EriNormalized,
                        risk_category = e.RiskCategory,
                        rank = e.Rank
                    }
                ).OrderBy(x => x.rank).ToListAsync();
 
                if (!results.Any())
                    return NotFound(new { message = $"No data found for scenario: {scenario}" });
 
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        [HttpGet("province/{name}")]
        public async Task<IActionResult> GetProvinceDetail(
            string name,
            [FromQuery] string scenario = "S1_Balanced")
        {
            try
            {
                var result = await (
                    from e in _context.EriScores
                    join m in _context.ProvinceMetadata on e.Province equals m.Province
                    where e.Province == name && e.ScenarioId == scenario
                    select new
                    {
                        province = e.Province,
                        latitude = m.Latitude,
                        longitude = m.Longitude,
                        afad_zone = m.AfadZone,
                        scenario_id = e.ScenarioId,
                        scenario_label = e.ScenarioLabel,
                        hazard_score = e.HazardScore,
                        vulnerability_score = e.VulnerabilityScore,
                        eri_normalized = e.EriNormalized,
                        risk_category = e.RiskCategory,
                        rank = e.Rank,
                        parameters = new
                        {
                            pga = m.PgaRaw,
                            pgv = m.PgvRaw,
                            ss = m.SsRaw,
                            hist_freq = m.HfRaw,
                            avg_depth = m.FdRaw,
                            pre1999_ratio = m.BaRaw,
                            sege_score = m.SegeScore,
                            population_density = m.PdRaw,
                            gdp_per_capita = m.GdpRaw
                        }
                    }
                ).FirstOrDefaultAsync();

                if (result == null)
                    return NotFound(new { message = $"{name} not found for scenario: {scenario}" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // --- YARDIMCI METOTLAR (GÜNCELLENDİ) ---

        // Fiyat Çevirici: "1.250,50" -> 1250.50 (Artık kuruşları da doğru anlıyor)
        private decimal ParsePrice(string? priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText)) return 0;

            // TL simgesi ve boşlukları temizle
            string cleanPrice = priceText.Replace("TL", "").Replace(" ", "").Trim();

            // Binlik ayırıcı noktayı sil, ondalık virgülü noktaya çevir
            if (cleanPrice.Contains(",") && cleanPrice.Contains("."))
                cleanPrice = cleanPrice.Replace(".", "").Replace(",", ".");
            else if (cleanPrice.Contains(","))
                cleanPrice = cleanPrice.Replace(",", ".");

            if (decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            // Eğer hala çeviremediyse sadece rakamları al ama son 2 haneyi kuruş say
            string onlyDigits = new string(priceText.Where(char.IsDigit).ToArray());
            return decimal.TryParse(onlyDigits, out decimal d) ? d : 0;
        }

        // Koordinat Çevirici
        private double ParseCoordinate(string? coord)
        {
            if (string.IsNullOrWhiteSpace(coord)) return 0;
            string cleanCoord = coord.Replace(",", ".");
            if (double.TryParse(cleanCoord, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;
            return 0;
        }

        private IActionResult ProcessResults(List<PropertyListing> data)
        {
            if (data == null || !data.Any()) return NotFound(new { message = "No matching listings found." });
            return Ok(data.Select(p => new {
                id = p.ListingId,
                address = $"{p.City} / {p.District} / {p.Neighborhood}",
                price = p.AskingPriceTry,
                details = new { rooms = p.RoomCount, floor = p.FloorLevel, sqm = p.GrossSqm },
                url = p.Url
            }));
        }
    }
}