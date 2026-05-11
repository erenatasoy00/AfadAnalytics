namespace AfadAnalytics.DTOs.Map
{
    public class DistrictMapDto
    {
        public string? City { get; set; }
        public string? District { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string? RiskScore { get; set; }
        public string? RiskCategory { get; set; }
        public int ListingCount { get; set; }
        public decimal AvgSalePrice { get; set; }
        public decimal AvgPricePerM2 { get; set; }
    }
}