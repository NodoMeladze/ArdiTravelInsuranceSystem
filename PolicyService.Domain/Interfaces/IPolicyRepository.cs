using PolicyService.Domain.Entities;

namespace PolicyService.Domain.Interfaces
{
    public interface IPolicyRepository
    {
        Task<Policy?> GetByIdAsync(Guid id);
        Task<Policy> AddAsync(Policy policy);
        Task<Policy> UpdateAsync(Policy policy);
        Task<Policy?> GetByPaymentIdAsync(Guid paymentId);
    }
}
