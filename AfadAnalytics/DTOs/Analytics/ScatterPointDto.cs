namespace AfadAnalytics.DTOs.Analytics;

public class ScatterPointDto
{
    public string? District { get; set; }
    public string? City { get; set; }
    public double Pga { get; set; }
    public double AvgSalePrice { get; set; }
    public string? RiskCategory { get; set; }
}