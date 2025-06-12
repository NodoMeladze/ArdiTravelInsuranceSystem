using PaymentService.Domain.DTOs;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto request);
        Task<PaymentDto?> GetPaymentAsync(Guid id);
    }
}
