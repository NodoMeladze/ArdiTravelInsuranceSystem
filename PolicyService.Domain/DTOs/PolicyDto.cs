using PolicyService.Domain.Enums;

namespace PolicyService.Domain.DTOs
{
    public class PolicyDto
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhoneNumber { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateTime TripStartDate { get; set; }
        public DateTime TripEndDate { get; set; }
        public int TripDurationDays { get; set; }
        public CoverageType CoverageType { get; set; }
        public PolicyStatus Status { get; set; }
        public decimal PremiumAmount { get; set; }
        public Guid PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
