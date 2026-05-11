using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AfadAnalytics.Models
{
    [Table("property_listings")]
    public class PropertyListing
    {
        [Key]
        [Column("listing_id")]
        public string ListingId { get; set; }

        [Column("listing_type")]
        public string? ListingType { get; set; }

        [Column("city")]
        public string? City { get; set; }

        [Column("district")]
        public string? District { get; set; }

        [Column("neighborhood")]
        public string? Neighborhood { get; set; }

        // SQL'de DOUBLE PRECISION olarak tutulduğu için C# tarafında double? yapıldı
        [Column("latitude")]
        public double? Latitude { get; set; }

        [Column("longitude")]
        public double? Longitude { get; set; }

        [Column("room_count")]
        public string? RoomCount { get; set; }

        [Column("bathroom_count")]
        public double? BathroomCount { get; set; }

        [Column("gross_sqm")]
        public double? GrossSqm { get; set; }

        [Column("net_sqm")]
        public double? NetSqm { get; set; }

        [Column("building_age_years")]
        public double? BuildingAgeYears { get; set; }

        [Column("floor_level")]
        public string? FloorLevel { get; set; }

        [Column("heating_type")]
        public string? HeatingType { get; set; }

        [Column("facade")]
        public string? Facade { get; set; }

        [Column("furnished")]
        public string? Furnished { get; set; }

        [Column("credit_eligible")]
        public string? CreditEligible { get; set; }

        [Column("asking_price_try")]
        public double? AskingPriceTry { get; set; }

        [Column("montlu_dues_try")] // SQL'de yazım hatasıyla "montlu" olarak açılmış, o yüzden aynen bağladık
        public double? MonthlyDuesTry { get; set; }

        [Column("seller")]
        public string? Seller { get; set; }

        [Column("square_meters")]
        public string? SquareMeters { get; set; }

        [Column("title_deed_type")]
        public string? TitleDeedType { get; set; }

        [Column("total_floors")]
        public string? TotalFloors { get; set; }

        [Column("usage_status")]
        public string? UsageStatus { get; set; }

        [Column("property_category")]
        public string? PropertyCategory { get; set; }

        [Column("gross_square_meters")]
        public string? GrossSquareMeters { get; set; }

        [Column("net_square_meters")]
        public string? NetSquareMeters { get; set; }

        [Column("title_status")]
        public string? TitleStatus { get; set; }

        [Column("building_age")]
        public string? BuildingAge { get; set; }

        [Column("afad_zone_1996")]
        public double? AfadZone1996 { get; set; }

        [Column("tdth_pga_475")]
        public double? TdthPga475 { get; set; }

        [Column("tdth_ss_475")]
        public double? TdthSs475 { get; set; }

        [Column("tdth_s1_475")]
        public double? TdthS1475 { get; set; }

        [Column("tdth_pgv_475")]
        public double? TdthPgv475 { get; set; }

        [Column("tdth_pga_2475")]
        public double? TdthPga2475 { get; set; }

        [Column("tdth_ss_2475")]
        public string? TdthSs2475 { get; set; }

        [Column("tdth_s1_2475")]
        public string? TdthS12475 { get; set; }

        [Column("tdth_pgv_2475")]
        public string? TdthPgv2475 { get; set; }

        [Column("tdth_pga_72")]
        public string? TdthPga72 { get; set; }

        [Column("tdth_pga_43")]
        public string? TdthPga43 { get; set; }

        [Column("catalog_total_eq")]
        public string? CatalogTotalEq { get; set; }

        [Column("catalog_max_mag")]
        public string? CatalogMaxMag { get; set; }

        [Column("catalog_mean_mag")]
        public string? CatalogMeanMag { get; set; }

        [Column("catalog_freq_m3_10yr")]
        public string? CatalogFreqM310Yr { get; set; }

        [Column("catalog_freq_m4_10yr")]
        public string? CatalogFreqM410Yr { get; set; }

        [Column("catalog_freq_m5_10yr")]
        public string? CatalogFreqM510Yr { get; set; }

        [Column("catalog_freq_m6_10yr")]
        public string? CatalogFreqM610Yr { get; set; }

        [Column("catalog_avg_depth_km")]
        public string? CatalogAvgDepthKm { get; set; }

        [Column("catalog_min_depth_km")]
        public string? CatalogMinDepthKm { get; set; }

        [Column("shallow_eq_pct")]
        public string? ShallowEqPct { get; set; }

        [Column("energy_release_total")]
        public string? EnergyReleaseTotal { get; set; }

        [Column("gr_b_value")]
        public string? GrBValue { get; set; }

        [Column("price_per_sqm_try")]
        public string? PricePerSqmTry { get; set; }

        [Column("risk_score")]
        public string? RiskScore { get; set; }

        [Column("risk_category")]
        public string? RiskCategory { get; set; }

        [Column("price_to_risk_ratio")]
        public string? PriceToRiskRatio { get; set; }

        [Column("rent_to_risk_ratio")]
        public string? RentToRiskRatio { get; set; }

        [Column("final_sqm")]
        public string? FinalSqm { get; set; }

      
       
    }
}