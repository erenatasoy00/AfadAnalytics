using AfadAnalytics.Data;
using AfadAnalytics.DTOs.Listing;
using Microsoft.EntityFrameworkCore;

namespace AfadAnalytics.Services.ListingService;

public class ListingsManager : IListingsService
{
    private readonly AppDbContext _context;

    public ListingsManager(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ListingSearchDto>> SearchAsync(string? city, string? district, string? rooms,
        string? floor, string? riskCategory, string? listingType, decimal? minPrice, decimal? maxPrice)
    {
        var query = _context.PropertyListings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => p.City != null && p.City.ToLower().Contains(city.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(district))
            query = query.Where(p => p.District != null && p.District.ToLower().Contains(district.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(rooms))
            query = query.Where(p => p.RoomCount != null && p.RoomCount.ToLower().Contains(rooms.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(floor))
            query = query.Where(p => p.FloorLevel != null && p.FloorLevel.ToLower().Contains(floor.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(listingType))
            query = query.Where(p => p.ListingType != null &&
                                     p.ListingType.ToLower().Contains(listingType.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(riskCategory))
        {
            var searchRisk = riskCategory.Trim().ToLower();
            query = from p in query
                    join r in _context.DistrictRisks on p.District equals r.DistrictName
                    where r.RiskCategory != null && r.RiskCategory.ToLower().Contains(searchRisk)
                    select p;
        }

        var allData = await query.ToListAsync();

        if (minPrice.HasValue)
            allData = allData.Where(p => (decimal?)(p.AskingPriceTry ?? 0) >= minPrice.Value).ToList();

        if (maxPrice.HasValue)
            allData = allData.Where(p => (decimal?)(p.AskingPriceTry ?? 0) <= maxPrice.Value).ToList();

        return allData.Select(p => new ListingSearchDto
        {
            ListingId = p.ListingId,
            City = p.City,
            District = p.District,
            Neighborhood = p.Neighborhood,
            AskingPrice = p.AskingPriceTry,
            RoomCount = p.RoomCount,
            FloorLevel = p.FloorLevel,
            GrossSqm = p.GrossSqm,
            BuildingAge = p.BuildingAge,
            BuildingAgeYears = p.BuildingAgeYears
        });
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(string? city, string? district, string? riskCategory,
        string? listingType, decimal? minPrice, decimal? maxPrice)
    {
        var query = from p in _context.PropertyListings
                    join r in _context.DistrictRisks on p.District equals r.DistrictName into rj
                    from r in rj.DefaultIfEmpty()
                    select new { Property = p, RiskCategory = r.RiskCategory };

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(q => q.Property.City != null && q.Property.City.ToLower().Contains(city.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(district))
            query = query.Where(q => q.Property.District != null && q.Property.District.ToLower().Contains(district.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(riskCategory))
            query = query.Where(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains(riskCategory.Trim().ToLower()));

        var allData = await query.ToListAsync();

        if (minPrice.HasValue)
            allData = allData.Where(q => (decimal?)(q.Property.AskingPriceTry ?? 0) >= minPrice.Value).ToList();

        if (maxPrice.HasValue)
            allData = allData.Where(q => (decimal?)(q.Property.AskingPriceTry ?? 0) <= maxPrice.Value).ToList();

        if (!string.IsNullOrWhiteSpace(listingType))
            allData = allData.Where(q => q.Property.ListingType != null &&
                                         q.Property.ListingType.ToLower().Contains(listingType.Trim().ToLower())).ToList();

        var total = allData.Count;

        return new DashboardSummaryDto
        {
            TotalListings = total,
            AveragePrice = total > 0 ? allData.Average(q => q.Property.AskingPriceTry ?? 0) : 0,
            HighRiskCount = allData.Count(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains("yüksek")),
            LowRiskCount = allData.Count(q => q.RiskCategory != null && q.RiskCategory.ToLower().Contains("düşük")),
            CityCount = allData.Select(q => q.Property.City).Distinct().Count(),
            DistrictCount = allData.Select(q => q.Property.District).Distinct().Count()
        };
    }
}