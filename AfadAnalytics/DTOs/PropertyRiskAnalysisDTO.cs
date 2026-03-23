namespace AfadAnalytics.DTOs
{
    public class PropertyRiskAnalysisDTO
    {
        public int Id { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public decimal OriginalPrice { get; set; }
        public double RiskScore { get; set; }
        public decimal AdjustedPrice { get; set; }
        public decimal ValueDifference { get; set; }
    }
}