using PaymentService.Application.Interfaces;
using PaymentService.Domain.DTOs;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.Mappers
{
    public class PaymentMapper : IPaymentMapper
    {
        public PaymentDto ToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                Method = payment.Method,
                IdempotencyKey = payment.IdempotencyKey,
                FailureReason = payment.FailureReason,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt
            };
        }

        public Payment ToEntity(ProcessPaymentDto dto)
        {
            return new Payment
            {
                Id = Guid.NewGuid(),
                Amount = dto.Amount,
                Currency = dto.Currency,
                Method = dto.PaymentMethod,
                Status = PaymentStatus.Pending,
                IdempotencyKey = dto.IdempotencyKey,
                CreatedAt = DateTime.UtcNow,
                CardNumber = dto.CardNumber,
                CardHolderName = dto.CardHolderName,
                PayPalEmail = dto.PayPalEmail
            };
        }

        public IEnumerable<PaymentDto> ToDtos(IEnumerable<Payment> payments)
        {
            return payments.Select(ToDto);
        }
    }
}
