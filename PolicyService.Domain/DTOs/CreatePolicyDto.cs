using PolicyService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PolicyService.Domain.DTOs
{
    public class CreatePolicyDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        public string? CustomerPhoneNumber { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime TripStartDate { get; set; }

        [Required]
        public DateTime TripEndDate { get; set; }

        [Required]
        public CoverageType CoverageType { get; set; }

        [Required]
        public Guid PaymentId { get; set; }
    }
}
