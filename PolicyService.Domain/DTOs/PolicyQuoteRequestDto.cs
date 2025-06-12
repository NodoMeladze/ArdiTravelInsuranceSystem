using PolicyService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PolicyService.Domain.DTOs
{
    public class PolicyQuoteRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime TripStartDate { get; set; }

        [Required]
        public DateTime TripEndDate { get; set; }

        [Required]
        public CoverageType CoverageType { get; set; }
    }
}
