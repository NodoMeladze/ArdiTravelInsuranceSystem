using PolicyService.Domain.DTOs;

namespace PolicyService.Domain.Interfaces
{
    public interface IPolicyValidator
    {
        Task<ValidationResult> ValidateAsync(CreatePolicyDto request);
        Task<ValidationResult> ValidateUpdateAsync(Guid policyId, UpdatePolicyStatusDto request);
        bool IsValidDestination(string destination);
        bool IsValidTripDuration(DateTime startDate, DateTime endDate);
    }
}
