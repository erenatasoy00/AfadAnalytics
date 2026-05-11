namespace AfadAnalytics.DTOs.Listing
{
    public class DashboardSummaryDto
    {
        public int TotalListings { get; set; }
        public double AveragePrice { get; set; }
        public int HighRiskCount { get; set; }
        public int LowRiskCount { get; set; }
        public int CityCount { get; set; }
        public int DistrictCount { get; set; }
    }
}