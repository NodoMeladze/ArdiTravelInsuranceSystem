using Microsoft.Extensions.Logging;
using PaymentService.Domain.DTOs;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.PaymentProcessors
{
    public abstract class BasePaymentProcessor(ILogger logger) : IPaymentProcessor
    {
        protected readonly ILogger Logger = logger;

        public abstract PaymentMethod SupportedMethod { get; }

        public virtual bool CanProcess(PaymentMethod method) => method == SupportedMethod;

        public abstract Task<PaymentProcessResult> ProcessAsync(ProcessPaymentDto request);

        protected virtual async Task<PaymentProcessResult> SimulatePaymentAsync(ProcessPaymentDto request)
        {
            // Simulate processing time
            await Task.Delay(Random.Shared.Next(100, 500));

            // Simulate occasional failures (10% failure rate)
            if (Random.Shared.NextDouble() < 0.1)
            {
                return PaymentProcessResult.Failure("Payment gateway temporarily unavailable");
            }

            // Generate mock transaction ID
            var transactionId = $"TXN_{SupportedMethod}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}";

            return PaymentProcessResult.Success(transactionId);
        }

        protected virtual void ValidatePaymentDetails(ProcessPaymentDto request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            if (string.IsNullOrWhiteSpace(request.Currency))
                throw new ArgumentException("Currency is required");
        }
    }
}
