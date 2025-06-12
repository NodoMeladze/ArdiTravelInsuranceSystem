using PolicyService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PolicyService.Domain.DTOs
{
    public class UpdatePolicyStatusDto
    {
        [Required]
        public PolicyStatus Status { get; set; }
    }
}
