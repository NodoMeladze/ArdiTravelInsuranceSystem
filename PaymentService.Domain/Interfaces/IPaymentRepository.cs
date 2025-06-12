using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid id);
        Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey);
        Task<Payment> AddAsync(Payment payment);
        Task<Payment> UpdateAsync(Payment payment);
    }
}
