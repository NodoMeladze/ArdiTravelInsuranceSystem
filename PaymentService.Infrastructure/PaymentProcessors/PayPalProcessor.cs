using Microsoft.Extensions.Logging;
using PaymentService.Domain.DTOs;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.PaymentProcessors
{
    public class PayPalProcessor(ILogger<PayPalProcessor> logger) : BasePaymentProcessor(logger)
    {
        public override PaymentMethod SupportedMethod => PaymentMethod.PayPal;

        public override async Task<PaymentProcessResult> ProcessAsync(ProcessPaymentDto request)
        {
            Logger.LogInformation("Processing PayPal payment for amount: {Amount} {Currency}",
                request.Amount, request.Currency);

            try
            {
                ValidatePaymentDetails(request);
                ValidatePayPalDetails(request);

                // Simulate PayPal API call
                await Task.Delay(200); // PayPal typically faster than card processing
                var result = await SimulatePaymentAsync(request);

                if (result.IsSuccess)
                {
                    result.Metadata["PayPalOrderId"] = $"PP_{DateTime.UtcNow:yyyyMMddHHmmss}";
                    Logger.LogInformation("PayPal payment processed successfully. TransactionId: {TransactionId}",
                        result.TransactionId);
                }
                else
                {
                    Logger.LogWarning("PayPal payment failed: {Reason}", result.FailureReason);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing PayPal payment");
                return PaymentProcessResult.Failure($"PayPal processing error: {ex.Message}");
            }
        }

        private static void ValidatePayPalDetails(ProcessPaymentDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PayPalEmail))
                throw new ArgumentException("PayPal email is required for PayPal payments");

            if (!request.PayPalEmail.Contains('@'))
                throw new ArgumentException("Invalid PayPal email format");
        }
    }
}