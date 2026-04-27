using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AfadAnalytics.Models
{
    [Table("risk_analyses")]
    public class RiskAnalysis
    {
        [Key]
        [Column("analysis_id")]
        public int AnalysisId { get; set; }

        [Column("property_id")]
        public int PropertyId { get; set; }

        [Column("fault_id")]
        public int FaultId { get; set; }

        [Column("distance_to_fault_meters")]
        public decimal? DistanceToFaultMeters { get; set; }

        [Column("ai_risk_score")]
        public decimal? AiRiskScore { get; set; }

        [Column("analyzed_at")]
        public DateTime? AnalyzedAt { get; set; }
    }
}