using PaymentService.Domain.DTOs;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentMapper
    {
        PaymentDto ToDto(Payment payment);
        Payment ToEntity(ProcessPaymentDto dto);
        IEnumerable<PaymentDto> ToDtos(IEnumerable<Payment> payments);
    }
}
