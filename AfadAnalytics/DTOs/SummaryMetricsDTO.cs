namespace AfadAnalytics.DTOs
{
    public class SummaryMetricsDTO
    {
        public int TotalListings { get; set; }
        public decimal AveragePrice { get; set; }
        public int HighRiskCount { get; set; }
        public int LowRiskCount { get; set; }
    }
}