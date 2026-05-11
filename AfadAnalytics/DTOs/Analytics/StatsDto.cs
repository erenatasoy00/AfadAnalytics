namespace AfadAnalytics.DTOs.Analytics;

public class StatsDto
{
    public int TotalListings { get; set; }
    public double AvgSalePrice { get; set; }
    public double AvgPricePerM2 { get; set; }
    public double AvgPga { get; set; }
    public int HighRiskCount { get; set; }
    public int DistrictCount { get; set; }
    public int CityCount { get; set; }
    public double AvgInvestmentScore { get; set; }
    public double AvgRiskScore { get; set; }
    public List<RiskDistributionDto>? RiskDistribution { get; set; }
    public List<AfadZoneDto>? AfadZoneDistribution { get; set; }
    public List<PgaBracketDto>? PgaDistribution { get; set; }
    public List<RiskPriceDto>? RiskPriceMatrix { get; set; }
}

public class RiskDistributionDto
{
    public string? RiskCategory { get; set; }
    public int Count { get; set; }
}

public class AfadZoneDto
{
    public string? Zone { get; set; }
    public int Count { get; set; }
}

public class PgaBracketDto
{
    public string? Label { get; set; }
    public int Count { get; set; }
}