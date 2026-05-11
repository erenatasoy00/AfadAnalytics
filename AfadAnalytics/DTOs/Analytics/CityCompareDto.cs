namespace AfadAnalytics.DTOs.Analytics;

public class CityCompareDto
{
    public string? City { get; set; }
    public string? RiskCategory { get; set; }
    public double AvgSalePrice { get; set; }
    public double AvgPricePerM2 { get; set; }
    public double AvgPga { get; set; }
    public double AvgInvestmentScore { get; set; }
    public int ListingCount { get; set; }
    public int DistrictCount { get; set; }
}