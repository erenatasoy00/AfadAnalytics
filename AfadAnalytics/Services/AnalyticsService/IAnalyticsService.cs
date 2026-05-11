using AfadAnalytics.DTOs.Analytics;

namespace AfadAnalytics.Services.AnalyticsService;

public interface IAnalyticsService
{
    Task<IEnumerable<CityPriceDto>> GetPriceByCityAsync(string? city, string? riskCategory, string? listingType);
    Task<IEnumerable<RiskPriceDto>> GetPriceByRiskAsync(string? city, string? listingType);
    Task<IEnumerable<ScatterPointDto>> GetScatterDataAsync(string? city);
    Task<IEnumerable<TopDistrictDto>> GetTopDistrictsAsync(string? city, string? riskCategory, int top = 4);
    Task<StatsDto> GetStatsAsync(string? city, string? riskCategory, string? listingType);
    Task<IEnumerable<CityCompareDto>> GetCityComparisonAsync();
}