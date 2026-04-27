using System.ComponentModel.DataAnnotations.Schema;

namespace AfadAnalytics.DTOs;

public class DistrictSummaryRaw
{
    [Column("city")]
    public string City { get; set; }
    
    [Column("district")]
    public string District { get; set; }
    
    [Column("latitude")]
    public string? Latitude { get; set; }
    
    [Column("longitude")]
    public string? Longitude { get; set; }
    
    [Column("asking_price_try")]
    public string? AskingPriceTry { get; set; }
    
    [Column("risk_category")]
    public string? RiskCategory { get; set; }
    
    [Column("risk_score")]
    public string? RiskScore { get; set; }
    
    [Column("price_per_sqm_try")]
    public string? PricePerSqm { get; set; }
}