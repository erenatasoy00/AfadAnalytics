using System.Globalization;
using AfadAnalytics.Data;
using AfadAnalytics.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AfadAnalytics.Services.MapService;

public class MapService
{
    public class MapService : IMapService
    {
        private readonly AppDbContext _context;

        public MapService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DistrictMapDto>> GetDistrictsAsync(string? city)
        {
            var sql = @"
                SELECT p.city, p.district, p.latitude, p.longitude,
                       p.asking_price_try, p.price_per_sqm_try,
                       r.risk_category, r.composite_risk_score as risk_score
                FROM property_listings p
                JOIN district_risks r 
                    ON unaccent(lower(p.district)) = unaccent(lower(r.district))
                WHERE p.district IS NOT NULL";

            if (!string.IsNullOrWhiteSpace(city))
                sql += $" AND lower(p.city) = lower('{city.Trim()}')";

            var rawData = await _context.Database
                .SqlQueryRaw<DistrictSummaryRaw>(sql)
                .ToListAsync();

            return rawData
                .GroupBy(x => new { x.City, x.District })
                .Select(g => new DistrictMapDto
                {
                    City = g.Key.City,
                    District = g.Key.District,
                    Lat = g.First().Latitude ?? 39.0,
                    Lng = g.First().Longitude ?? 35.0,
                    RiskScore = g.First().RiskScore,
                    RiskCategory = g.First().RiskCategory ?? "Bilinmiyor",
                    ListingCount = g.Count(),
                    AvgSalePrice = Math.Round((decimal)(g.Average(x => x.AskingPriceTry) ?? 0), 2),
                    AvgPricePerM2 = g.Any(x => !string.IsNullOrWhiteSpace(x.PricePerSqmTry))
                        ? Math.Round((decimal)g.Where(x => !string.IsNullOrWhiteSpace(x.PricePerSqmTry))
                            .Average(x => ParsePrice(x.PricePerSqmTry)), 2)
                        : 0
                });
        }

        public async Task<IEnumerable<ProvinceMapDto>> GetProvincesAsync(string scenarioId)
        {
            var results = await (
                from e in _context.EriScores
                join m in _context.ProvinceMetadata on e.Province equals m.Province
                where e.ScenarioId == scenarioId
                select new ProvinceMapDto
                {
                    Province = e.Province,
                    Latitude = m.Latitude ?? 39.0,
                    Longitude = m.Longitude ?? 35.0,
                    AfadZone = m.AfadZone ?? 0,
                    ScenarioId = e.ScenarioId,
                    ScenarioLabel = e.ScenarioLabel,
                    HazardScore = e.HazardScore,
                    VulnerabilityScore = e.VulnerabilityScore,
                    EriNormalized = e.EriNormalized,
                    RiskCategory = e.RiskCategory,
                    Rank = e.Rank
                }
            ).OrderBy(x => x.Rank).ToListAsync();

            return results;
        }

        public async Task<ProvinceDetailDto?> GetProvinceDetailAsync(string name, string scenarioId)
        {
            var result = await (
                from e in _context.EriScores
                join m in _context.ProvinceMetadata on e.Province equals m.Province
                where e.Province == name && e.ScenarioId == scenarioId
                select new ProvinceDetailDto
                {
                    Province = e.Province,
                    Latitude = m.Latitude ?? 39.0,
                    Longitude = m.Longitude ?? 35.0,
                    AfadZone = m.AfadZone ?? 0,
                    ScenarioId = e.ScenarioId,
                    ScenarioLabel = e.ScenarioLabel,
                    HazardScore = e.HazardScore,
                    VulnerabilityScore = e.VulnerabilityScore,
                    EriNormalized = e.EriNormalized,
                    RiskCategory = e.RiskCategory,
                    Rank = e.Rank,
                    Parameters = new ProvinceParameters
                    {
                        Pga = m.PgaRaw,
                        Pgv = m.PgvRaw,
                        Ss = m.SsRaw,
                        HistFreq = m.HfRaw,
                        AvgDepth = m.FdRaw,
                        Pre1999Ratio = m.BaRaw,
                        SegeScore = m.SegeScore,
                        PopulationDensity = m.PdRaw,
                        GdpPerCapita = m.GdpRaw
                    }
                }
            ).FirstOrDefaultAsync();

            return result;
        }

        private decimal ParsePrice(string? priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText)) return 0;
            string cleanPrice = priceText.Replace("TL", "").Replace(" ", "").Trim();
            if (cleanPrice.Contains(",") && cleanPrice.Contains("."))
                cleanPrice = cleanPrice.Replace(".", "").Replace(",", ".");
            else if (cleanPrice.Contains(","))
                cleanPrice = cleanPrice.Replace(",", ".");
            if (decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;
            return 0;
        }
    }
}