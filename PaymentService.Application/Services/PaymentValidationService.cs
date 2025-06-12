using PaymentService.Domain.DTOs;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Application.Services
{
    public class PaymentValidationService
    {
        private readonly HashSet<string> _supportedCurrencies = ["USD", "EUR", "GEL"];
        private readonly decimal _maxPaymentAmount = 10000m;
        private readonly decimal _minPaymentAmount = 0.01m;

        public void ValidatePaymentRequest(ProcessPaymentDto request)
        {
            ValidateAmount(request.Amount);
            ValidateCurrency(request.Currency);
            ValidatePaymentMethod(request.PaymentMethod);
            ValidatePaymentMethodSpecificDetails(request);
        }

        private void ValidateAmount(decimal amount)
        {
            if (amount < _minPaymentAmount)
                throw new PaymentProcessingException($"Payment amount must be at least {_minPaymentAmount}");

            if (amount > _maxPaymentAmount)
                throw new PaymentProcessingException($"Payment amount cannot exceed {_maxPaymentAmount}");
        }

        private void ValidateCurrency(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new PaymentProcessingException("Currency is required");

            if (!_supportedCurrencies.Contains(currency.ToUpper()))
                throw new PaymentProcessingException($"Currency {currency} is not supported");
        }

        private static void ValidatePaymentMethod(PaymentMethod method)
        {
            if (!Enum.IsDefined(typeof(PaymentMethod), method))
                throw new PaymentProcessingException($"Invalid payment method: {method}");
        }

        private static void ValidatePaymentMethodSpecificDetails(ProcessPaymentDto request)
        {
            switch (request.PaymentMethod)
            {
                case PaymentMethod.CreditCard:
                case PaymentMethod.DebitCard:
                    ValidateCreditCardDetails(request);
                    break;
                case PaymentMethod.PayPal:
                    ValidatePayPalDetails(request);
                    break;
            }
        }

        private static void ValidateCreditCardDetails(ProcessPaymentDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CardNumber))
                throw new PaymentProcessingException("Card number is required for card payments");

            if (string.IsNullOrWhiteSpace(request.CardHolderName))
                throw new PaymentProcessingException("Card holder name is required for card payments");

            if (!IsValidCardNumber(request.CardNumber))
                throw new PaymentProcessingException("Invalid card number");
        }

        private static bool IsValidCardNumber(string cardNumber)
        {
            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            if (!cardNumber.All(char.IsDigit))
                return false;

            if (cardNumber.Length < 13 || cardNumber.Length > 19)
                return false;

            return true;
        }

        private static void ValidatePayPalDetails(ProcessPaymentDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PayPalEmail))
                throw new PaymentProcessingException("PayPal email is required for PayPal payments");

            if (!request.PayPalEmail.Contains('@'))
                throw new PaymentProcessingException("Invalid PayPal email format");
        }
    }
}
