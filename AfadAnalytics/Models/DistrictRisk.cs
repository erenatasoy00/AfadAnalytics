using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AfadAnalytics.Models
{
    [Table("district_risks")]
    public class DistrictRisk
    {
        [Key]
        [Column("district")]
        public string DistrictName { get; set; }

        [Column("province")]
        public string? Province { get; set; }

        [Column("risk_category")]
        public string? RiskCategory { get; set; }

        [Column("risk_score")]
        public string? RiskScore { get; set; }
    }
}