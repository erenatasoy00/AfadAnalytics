namespace AfadAnalytics.DTOs.Listing
{
    public class ListingSearchDto
    {
        public string? ListingId { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Neighborhood { get; set; }
        public double? AskingPrice { get; set; }
        public string? RoomCount { get; set; }
        public string? FloorLevel { get; set; }
        public double? GrossSqm { get; set; }
        public string? BuildingAge { get; set; }
        public double? BuildingAgeYears { get; set; }
    }
}