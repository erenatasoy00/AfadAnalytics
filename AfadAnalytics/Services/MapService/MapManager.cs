using System.Globalization;
using AfadAnalytics.Data;
using AfadAnalytics.DTOs.Map;
using Microsoft.EntityFrameworkCore;

namespace AfadAnalytics.Services.MapService;

public class MapManager : IMapService
    {
        private readonly AppDbContext _context;

        public MapManager(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DistrictMapDto>> GetDistrictsAsync(string? city)
        {
            var cityFilter = string.IsNullOrWhiteSpace(city) ? "" 
                : $" AND unaccent(lower(p.city)) = unaccent(lower('{city.Trim()}'))";

            var sql = $@"
        SELECT 
            p.city,
            p.district,
            AVG(p.latitude) as lat,
            AVG(p.longitude) as lng,
            MAX(r.risk_category) as risk_category,
            MAX(r.composite_risk_score) as risk_score,
            COUNT(*) as listing_count,
            AVG(p.asking_price_try) as avg_sale_price,
            AVG(CAST(p.price_per_sqm_try AS double precision)) as avg_price_per_m2
        FROM property_listings p
        LEFT JOIN district_risks r 
            ON unaccent(lower(p.district)) = unaccent(lower(r.district))
        WHERE p.district IS NOT NULL
          AND p.latitude BETWEEN 35 AND 43
          AND p.longitude BETWEEN 25 AND 45
          {cityFilter}
        GROUP BY p.city, p.district
        ORDER BY p.city, p.district";

            var rawData = await _context.Database
                .SqlQueryRaw<DistrictSummaryRaw>(sql)
                .ToListAsync();

            return rawData.Select(g => new DistrictMapDto
            {
                City = g.City,
                District = g.District,
                Lat = g.Latitude ?? 39.0,
                Lng = g.Longitude ?? 35.0,
                RiskScore = g.RiskScore,
                RiskCategory = g.RiskCategory ?? "Bilinmiyor",
                ListingCount = (int)(g.ListingCount ?? 0),
                AvgSalePrice = Math.Round((decimal)(g.AvgSalePrice ?? 0), 2),
                AvgPricePerM2 = Math.Round((decimal)(g.AvgPricePerM2 ?? 0), 2)
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