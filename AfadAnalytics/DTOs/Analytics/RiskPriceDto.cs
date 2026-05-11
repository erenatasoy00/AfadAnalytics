namespace AfadAnalytics.DTOs.Analytics;

public class RiskPriceDto
{
    public string? RiskCategory { get; set; }
    public double AvgSalePrice { get; set; }
    public double AvgPricePerM2 { get; set; }
    public int ListingCount { get; set; }
}