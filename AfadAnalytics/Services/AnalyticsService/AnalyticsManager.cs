using AfadAnalytics.Data;
using AfadAnalytics.DTOs.Analytics;
using Microsoft.EntityFrameworkCore;

namespace AfadAnalytics.Services.AnalyticsService
{
    public class AnalyticsManager : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsManager(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CityPriceDto>> GetPriceByCityAsync(string? city, string? riskCategory, string? listingType)
        {
            var query = _context.PropertyListings.AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(p => p.City != null && p.City.ToLower().Contains(city.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(listingType))
                query = query.Where(p => p.ListingType != null && p.ListingType.ToLower().Contains(listingType.Trim().ToLower()));

            var allData = await query.ToListAsync();

            return allData
                .Where(p => p.City != null && p.AskingPriceTry > 0)
                .GroupBy(p => p.City)
                .Select(g => new CityPriceDto
                {
                    City = g.Key,
                    AvgSalePrice = Math.Round(g.Average(p => p.AskingPriceTry ?? 0), 2),
                    AvgPricePerM2 = Math.Round(g.Where(p => !string.IsNullOrEmpty(p.PricePerSqmTry))
                        .Select(p => ParsePrice(p.PricePerSqmTry))
                        .DefaultIfEmpty(0)
                        .Average(), 2),
                    ListingCount = g.Count()
                })
                .OrderByDescending(c => c.AvgSalePrice)
                .Take(20);
        }

        public async Task<IEnumerable<RiskPriceDto>> GetPriceByRiskAsync(string? city, string? listingType)
        {
            var allListings = await _context.PropertyListings.ToListAsync();
            var allRisks = await _context.DistrictRisks.ToListAsync();

            var joined = allListings
                .GroupJoin(allRisks, p => p.District, r => r.DistrictName,
                    (p, risks) => new { Property = p, Risk = risks.FirstOrDefault() })
                .ToList();

            if (!string.IsNullOrWhiteSpace(city))
                joined = joined.Where(q => q.Property.City != null && q.Property.City.ToLower().Contains(city.Trim().ToLower())).ToList();

            if (!string.IsNullOrWhiteSpace(listingType))
                joined = joined.Where(q => q.Property.ListingType != null && q.Property.ListingType.ToLower().Contains(listingType.Trim().ToLower())).ToList();

            return joined
                .Where(q => q.Risk?.RiskCategory != null && q.Property.AskingPriceTry > 0)
                .GroupBy(q => q.Risk!.RiskCategory)
                .Select(g => new RiskPriceDto
                {
                    RiskCategory = g.Key,
                    AvgSalePrice = Math.Round(g.Average(q => q.Property.AskingPriceTry ?? 0), 2),
                    AvgPricePerM2 = Math.Round(g.Where(q => !string.IsNullOrEmpty(q.Property.PricePerSqmTry))
                        .Select(q => ParsePrice(q.Property.PricePerSqmTry))
                        .DefaultIfEmpty(0)
                        .Average(), 2),
                    ListingCount = g.Count()
                });
        }

        public async Task<IEnumerable<ScatterPointDto>> GetScatterDataAsync(string? city)
        {
            var allListings = await _context.PropertyListings
                .Where(p => p.TdthPga475 != null && p.AskingPriceTry > 0 && p.District != null)
                .ToListAsync();

            var allRisks = await _context.DistrictRisks.ToListAsync();

            var joined = allListings
                .GroupJoin(allRisks, p => p.District, r => r.DistrictName,
                    (p, risks) => new { Property = p, Risk = risks.FirstOrDefault() })
                .ToList();

            if (!string.IsNullOrWhiteSpace(city))
                joined = joined.Where(q => q.Property.City != null && q.Property.City.ToLower().Contains(city.Trim().ToLower())).ToList();

            return joined
                .GroupBy(q => new { q.Property.City, q.Property.District })
                .Select(g => new ScatterPointDto
                {
                    District = g.Key.District,
                    City = g.Key.City,
                    Pga = g.First().Property.TdthPga475 ?? 0,
                    AvgSalePrice = Math.Round(g.Average(q => q.Property.AskingPriceTry ?? 0), 2),
                    RiskCategory = g.First().Risk?.RiskCategory ?? "Bilinmiyor"
                });
        }

        public async Task<IEnumerable<TopDistrictDto>> GetTopDistrictsAsync(string? city, string? riskCategory, int top = 4)
        {
            var allListings = await _context.PropertyListings
                .Where(p => p.District != null)
                .ToListAsync();

            var allRisks = await _context.DistrictRisks.ToListAsync();

            var joined = allListings
                .GroupJoin(allRisks, p => p.District, r => r.DistrictName,
                    (p, risks) => new { Property = p, Risk = risks.FirstOrDefault() })
                .ToList();

            if (!string.IsNullOrWhiteSpace(city))
                joined = joined.Where(q => q.Property.City != null && q.Property.City.ToLower().Contains(city.Trim().ToLower())).ToList();

            if (!string.IsNullOrWhiteSpace(riskCategory))
                joined = joined.Where(q => q.Risk?.RiskCategory != null && q.Risk.RiskCategory.ToLower().Contains(riskCategory.Trim().ToLower())).ToList();

            return joined
                .GroupBy(q => new { q.Property.City, q.Property.District })
                .Select(g => new TopDistrictDto
                {
                    District = g.Key.District,
                    City = g.Key.City,
                    ListingCount = g.Count(),
                    AvgPricePerM2 = Math.Round(g.Where(q => !string.IsNullOrEmpty(q.Property.PricePerSqmTry))
                        .Select(q => ParsePrice(q.Property.PricePerSqmTry))
                        .DefaultIfEmpty(0)
                        .Average(), 2),
                    RiskCategory = g.First().Risk?.RiskCategory ?? "Bilinmiyor"
                })
                .OrderByDescending(d => d.ListingCount)
                .Take(top);
        }

        public async Task<StatsDto> GetStatsAsync(string? city, string? riskCategory, string? listingType)
        {
            var allListings = await _context.PropertyListings.ToListAsync();
            var allRisks = await _context.DistrictRisks.ToListAsync();

            var joined = allListings
                .GroupJoin(allRisks, p => p.District, r => r.DistrictName,
                    (p, risks) => new { Property = p, Risk = risks.FirstOrDefault() })
                .ToList();

            if (!string.IsNullOrWhiteSpace(city))
                joined = joined.Where(q => q.Property.City != null && q.Property.City.ToLower().Contains(city.Trim().ToLower())).ToList();

            if (!string.IsNullOrWhiteSpace(riskCategory))
                joined = joined.Where(q => q.Risk?.RiskCategory != null && q.Risk.RiskCategory.ToLower().Contains(riskCategory.Trim().ToLower())).ToList();

            if (!string.IsNullOrWhiteSpace(listingType))
                joined = joined.Where(q => q.Property.ListingType != null && q.Property.ListingType.ToLower().Contains(listingType.Trim().ToLower())).ToList();

            var total = joined.Count;

            var riskDist = joined
                .Where(q => q.Risk?.RiskCategory != null)
                .GroupBy(q => q.Risk!.RiskCategory)
                .Select(g => new RiskDistributionDto { RiskCategory = g.Key, Count = g.Count() })
                .ToList();

            var afadDist = joined
                .Where(q => q.Property.AfadZone1996 != null)
                .GroupBy(q => q.Property.AfadZone1996!.Value.ToString("0"))
                .Select(g => new AfadZoneDto { Zone = $"Bölge {g.Key}", Count = g.Count() })
                .OrderByDescending(z => z.Count)
                .ToList();

            var pgaDist = new List<PgaBracketDto>
            {
                new() { Label = "< 0.1g",   Count = joined.Count(q => q.Property.TdthPga475 < 0.1) },
                new() { Label = "0.1–0.2g", Count = joined.Count(q => q.Property.TdthPga475 >= 0.1 && q.Property.TdthPga475 < 0.2) },
                new() { Label = "0.2–0.3g", Count = joined.Count(q => q.Property.TdthPga475 >= 0.2 && q.Property.TdthPga475 < 0.3) },
                new() { Label = "0.3–0.4g", Count = joined.Count(q => q.Property.TdthPga475 >= 0.3 && q.Property.TdthPga475 < 0.4) },
                new() { Label = "> 0.4g",   Count = joined.Count(q => q.Property.TdthPga475 >= 0.4) }
            };

            var riskPriceMatrix = joined
                .Where(q => q.Risk?.RiskCategory != null && q.Property.AskingPriceTry > 0)
                .GroupBy(q => q.Risk!.RiskCategory)
                .Select(g => new RiskPriceDto
                {
                    RiskCategory = g.Key,
                    AvgSalePrice = Math.Round(g.Average(q => q.Property.AskingPriceTry ?? 0), 2),
                    AvgPricePerM2 = Math.Round(g.Where(q => !string.IsNullOrEmpty(q.Property.PricePerSqmTry))
                        .Select(q => ParsePrice(q.Property.PricePerSqmTry))
                        .DefaultIfEmpty(0)
                        .Average(), 2),
                    ListingCount = g.Count()
                }).ToList();

            return new StatsDto
            {
                TotalListings = total,
                AvgSalePrice = total > 0 ? Math.Round(joined.Average(q => q.Property.AskingPriceTry ?? 0), 2) : 0,
                AvgPricePerM2 = Math.Round(joined.Where(q => !string.IsNullOrEmpty(q.Property.PricePerSqmTry))
                    .Select(q => ParsePrice(q.Property.PricePerSqmTry))
                    .DefaultIfEmpty(0)
                    .Average(), 2),
                AvgPga = total > 0 ? Math.Round(joined.Where(q => q.Property.TdthPga475 != null)
                    .Average(q => q.Property.TdthPga475 ?? 0), 4) : 0,
                HighRiskCount = joined.Count(q => q.Risk?.RiskCategory != null && q.Risk.RiskCategory.ToLower().Contains("yüksek")),
                DistrictCount = joined.Select(q => q.Property.District).Distinct().Count(),
                CityCount = joined.Select(q => q.Property.City).Distinct().Count(),
                AvgInvestmentScore = 70,
                AvgRiskScore = joined.Where(q => q.Risk?.RiskScore != null)
                    .Select(q => ParsePrice(q.Risk!.RiskScore))
                    .DefaultIfEmpty(0)
                    .Average(),
                RiskDistribution = riskDist,
                AfadZoneDistribution = afadDist,
                PgaDistribution = pgaDist,
                RiskPriceMatrix = riskPriceMatrix
            };
        }

        public async Task<IEnumerable<CityCompareDto>> GetCityComparisonAsync()
        {
            var allListings = await _context.PropertyListings
                .Where(p => p.City != null)
                .ToListAsync();

            var allRisks = await _context.DistrictRisks.ToListAsync();

            var joined = allListings
                .GroupJoin(allRisks, p => p.District, r => r.DistrictName,
                    (p, risks) => new { Property = p, Risk = risks.FirstOrDefault() })
                .ToList();

            return joined
                .GroupBy(q => q.Property.City)
                .Select(g => new CityCompareDto
                {
                    City = g.Key,
                    RiskCategory = g.Where(q => q.Risk?.RiskCategory != null)
                        .GroupBy(q => q.Risk!.RiskCategory)
                        .OrderByDescending(rg => rg.Count())
                        .FirstOrDefault()?.Key ?? "Bilinmiyor",
                    AvgSalePrice = Math.Round(g.Where(q => q.Property.AskingPriceTry > 0)
                        .Average(q => q.Property.AskingPriceTry ?? 0), 2),
                    AvgPricePerM2 = Math.Round(g.Where(q => !string.IsNullOrEmpty(q.Property.PricePerSqmTry))
                        .Select(q => ParsePrice(q.Property.PricePerSqmTry))
                        .DefaultIfEmpty(0)
                        .Average(), 2),
                    AvgPga = Math.Round(g.Where(q => q.Property.TdthPga475 != null)
                        .Average(q => q.Property.TdthPga475 ?? 0), 4),
                    AvgInvestmentScore = 70,
                    ListingCount = g.Count(),
                    DistrictCount = g.Select(q => q.Property.District).Distinct().Count()
                })
                .OrderByDescending(c => c.ListingCount);
        }

        private double ParsePrice(string? priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText)) return 0;
            string cleanPrice = priceText.Replace("TL", "").Replace(" ", "").Trim();
            if (cleanPrice.Contains(",") && cleanPrice.Contains("."))
                cleanPrice = cleanPrice.Replace(".", "").Replace(",", ".");
            else if (cleanPrice.Contains(","))
                cleanPrice = cleanPrice.Replace(",", ".");
            if (double.TryParse(cleanPrice, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result))
                return result;
            return 0;
        }
    }
}