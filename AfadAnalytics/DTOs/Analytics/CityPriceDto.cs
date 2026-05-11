namespace AfadAnalytics.DTOs.Analytics;

public class CityPriceDto
{
    public string? City { get; set; }
    public double AvgSalePrice { get; set; }
    public double AvgPricePerM2 { get; set; }
    public int ListingCount { get; set; }
}