namespace AfadAnalytics.DTOs.Analytics;

public class TopDistrictDto
{
    public string? District { get; set; }
    public string? City { get; set; }
    public int ListingCount { get; set; }
    public double AvgPricePerM2 { get; set; }
    public string? RiskCategory { get; set; }
}