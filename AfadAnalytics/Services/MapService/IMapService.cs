using AfadAnalytics.DTOs.Map;

namespace AfadAnalytics.Services.MapService;

public interface IMapService
{
    Task<IEnumerable<DistrictMapDto>> GetDistrictsAsync(string? city);
    Task<IEnumerable<ProvinceMapDto>> GetProvincesAsync(string scenarioId);
    Task<ProvinceDetailDto?> GetProvinceDetailAsync(string name, string scenarioId);
}