using AfadAnalytics.DTOs.Listing;

namespace AfadAnalytics.Services.ListingService;

public interface IListingsService
{
    Task<IEnumerable<ListingSearchDto>> SearchAsync(string? city, string? district, string? rooms, string? floor,
        string? riskCategory, string? listingType, decimal? minPrice, decimal? maxPrice);

    Task<DashboardSummaryDto> GetDashboardSummaryAsync(string? city, string? district, string? riskCategory, 
        string? listingType, decimal? minPrice, decimal? maxPrice);
}