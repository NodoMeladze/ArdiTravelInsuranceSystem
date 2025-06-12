using PaymentService.Domain.DTOs;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Interfaces
{
    public interface IPaymentProcessor
    {
        Task<PaymentProcessResult> ProcessAsync(ProcessPaymentDto request);
        PaymentMethod SupportedMethod { get; }
        bool CanProcess(PaymentMethod method);
    }
}
