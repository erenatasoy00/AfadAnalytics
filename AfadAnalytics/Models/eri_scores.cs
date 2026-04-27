using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AfadAnalytics.Models
{
    [Table("eri_scores")]
    public class EriScore
    {
        [Key]
        [Column("province")]
        public string Province { get; set; }
 
        [Key]
        [Column("scenario_id")]
        public string ScenarioId { get; set; }
 
        [Column("latitude")]
        public double? Latitude { get; set; }
 
        [Column("longitude")]
        public double? Longitude { get; set; }
 
        [Column("afad_zone")]
        public int? AfadZone { get; set; }
 
        [Column("scenario_label")]
        public string? ScenarioLabel { get; set; }
 
        [Column("hazard_score")]
        public double? HazardScore { get; set; }
 
        [Column("vulnerability_score")]
        public double? VulnerabilityScore { get; set; }
 
        [Column("eri_raw")]
        public double? EriRaw { get; set; }
 
        [Column("eri_normalized")]
        public double? EriNormalized { get; set; }
 
        [Column("risk_category")]
        public string? RiskCategory { get; set; }
 
        [Column("rank")]
        public int? Rank { get; set; }
    }
}