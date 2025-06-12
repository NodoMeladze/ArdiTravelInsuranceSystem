using PolicyService.Domain.DTOs;
using PolicyService.Domain.Enums;

namespace PolicyService.Application.Interfaces
{
    public interface IPolicyService
    {
        Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto request);
        Task<PolicyDto?> GetPolicyAsync(Guid id);
        Task<PolicyDto> UpdatePolicyStatusAsync(Guid id, PolicyStatus status);
        Task<PolicyQuoteDto> GetQuote(PolicyQuoteRequestDto request);
        Task<PolicyDto?> GetPolicyByPaymentIdAsync(Guid paymentId);
    }
}
