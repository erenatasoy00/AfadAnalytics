namespace AfadAnalytics.DTOs.Map
{
    public class ProvinceMapDto
    {
        public string? Province { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int AfadZone { get; set; }
        public string? ScenarioId { get; set; }
        public string? ScenarioLabel { get; set; }
        public double? HazardScore { get; set; }
        public double? VulnerabilityScore { get; set; }
        public double? EriNormalized { get; set; }
        public string? RiskCategory { get; set; }
        public int? Rank { get; set; }
    }
}