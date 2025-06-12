using PolicyService.Domain.DTOs;

namespace PolicyService.Application.Interfaces
{
    public interface IPaymentServiceClient
    {
        Task<PaymentResponseDto?> GetPaymentStatusAsync(Guid paymentId);
        Task<bool> IsPaymentCompletedAsync(Guid paymentId);
    }
}
