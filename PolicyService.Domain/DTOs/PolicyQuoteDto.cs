namespace PolicyService.Domain.DTOs
{
    public class PolicyQuoteDto
    {
        public decimal Premium { get; set; }
        public string Currency { get; set; } = "GEL";
        public DateTime ValidUntil { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string CoverageType { get; set; } = string.Empty;
        public int TripDurationDays { get; set; }
    }
}
