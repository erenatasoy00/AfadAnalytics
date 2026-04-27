using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AfadAnalytics.Models
{
    [Table("province_metadata")]
    public class ProvinceMetadata
    {
        [Key]
        [Column("province")]
        public string Province { get; set; }
 
        [Column("latitude")]
        public double? Latitude { get; set; }
 
        [Column("longitude")]
        public double? Longitude { get; set; }
 
        [Column("afad_zone")]
        public int? AfadZone { get; set; }
 
        [Column("pga_raw")]
        public double? PgaRaw { get; set; }
 
        [Column("pgv_raw")]
        public double? PgvRaw { get; set; }
 
        [Column("ss_raw")]
        public double? SsRaw { get; set; }
 
        [Column("hf_raw")]
        public double? HfRaw { get; set; }
 
        [Column("fd_raw")]
        public double? FdRaw { get; set; }
 
        [Column("ba_raw")]
        public double? BaRaw { get; set; }
 
        [Column("sege_score")]
        public double? SegeScore { get; set; }
 
        [Column("pd_raw")]
        public double? PdRaw { get; set; }
 
        [Column("gdp_raw")]
        public double? GdpRaw { get; set; }
    }
}