namespace AfadAnalytics.DTOs
{
    public class ProvinceDetailDto
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
        public ProvinceParameters? Parameters { get; set; }
    }

    public class ProvinceParameters
    {
        public double? Pga { get; set; }
        public double? Pgv { get; set; }
        public double? Ss { get; set; }
        public double? HistFreq { get; set; }
        public double? AvgDepth { get; set; }
        public double? Pre1999Ratio { get; set; }
        public double? SegeScore { get; set; }
        public double? PopulationDensity { get; set; }
        public double? GdpPerCapita { get; set; }
    }
}