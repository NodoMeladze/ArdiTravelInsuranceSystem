using Microsoft.Extensions.Logging;
using PaymentService.Domain.DTOs;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.PaymentProcessors
{
    public class CreditCardProcessor(ILogger<CreditCardProcessor> logger) : BasePaymentProcessor(logger)
    {
        public override PaymentMethod SupportedMethod => PaymentMethod.CreditCard;

        public override async Task<PaymentProcessResult> ProcessAsync(ProcessPaymentDto request)
        {
            Logger.LogInformation("Processing credit card payment for amount: {Amount} {Currency}",
                request.Amount, request.Currency);

            try
            {
                ValidatePaymentDetails(request);
                ValidateCreditCardDetails(request);

                // Simulate credit card processing
                var result = await SimulatePaymentAsync(request);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Credit card payment processed successfully. TransactionId: {TransactionId}",
                        result.TransactionId);
                }
                else
                {
                    Logger.LogWarning("Credit card payment failed: {Reason}", result.FailureReason);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing credit card payment");
                return PaymentProcessResult.Failure($"Credit card processing error: {ex.Message}");
            }
        }

        private static void ValidateCreditCardDetails(ProcessPaymentDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CardNumber))
                throw new ArgumentException("Card number is required for credit card payments");

            if (string.IsNullOrWhiteSpace(request.CardHolderName))
                throw new ArgumentException("Card holder name is required for credit card payments");

            if (request.CardNumber.Length < 13 || request.CardNumber.Length > 19)
                throw new ArgumentException("Invalid card number format");
        }
    }
}
