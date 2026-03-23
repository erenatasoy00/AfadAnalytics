using NetTopologySuite.Geometries;
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

        [Column("url")]
        public string? Url { get; set; }

        [Column("date")]
        public string? Date { get; set; }

        [Column("listing_type")]
        public string? ListingType { get; set; }

        [Column("city")]
        public string? City { get; set; }

        [Column("district")]
        public string? District { get; set; }

        [Column("neighborhood")]
        public string? Neighborhood { get; set; }

        [Column("latitude")]
        public string? Latitude { get; set; }

        [Column("longitude")]
        public string? Longitude { get; set; }

        [Column("asking_price_try")]
        public string? AskingPriceTry { get; set; }

        [Column("room_count")]
        public string? RoomCount { get; set; }

        [Column("gross_sqm")]
        public string? GrossSqm { get; set; }

        [Column("net_sqm")]
        public string? NetSqm { get; set; }

        [Column("building_age")]
        public string? BuildingAge { get; set; }

        [Column("floor_level")]
        public string? FloorLevel { get; set; }

        [Column("total_floors")]
        public string? TotalFloors { get; set; }

        [Column("heating_type")]
        public string? HeatingType { get; set; }

        [Column("platform")]
        public string? Platform { get; set; }

       // [Column("geom", TypeName = "geometry")]
        //public Point? Geom { get; set; }
    }
}