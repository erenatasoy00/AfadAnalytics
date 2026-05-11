using System.ComponentModel.DataAnnotations.Schema;

namespace AfadAnalytics.DTOs.Map
{
    public class DistrictSummaryRaw
    {
        [Column("city")]
        public string? City { get; set; }

        [Column("district")]
        public string? District { get; set; }

        [Column("lat")]
        public double? Latitude { get; set; }

        [Column("lng")]
        public double? Longitude { get; set; }

        [Column("avg_sale_price")]
        public double? AvgSalePrice { get; set; }

        [Column("avg_price_per_m2")]
        public double? AvgPricePerM2 { get; set; }

        [Column("risk_category")]
        public string? RiskCategory { get; set; }

        [Column("risk_score")]
        public string? RiskScore { get; set; }

        [Column("listing_count")]
        public long? ListingCount { get; set; }
    }
}