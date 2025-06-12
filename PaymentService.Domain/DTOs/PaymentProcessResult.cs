using System.Text.Json.Serialization;

namespace PaymentService.Domain.DTOs
{
    public class PaymentProcessResult
    {
        public bool IsSuccess { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> Metadata { get; set; } = [];

        public static PaymentProcessResult Success(string transactionId)
        {
            return new PaymentProcessResult
            {
                IsSuccess = true,
                TransactionId = transactionId
            };
        }

        public static PaymentProcessResult Failure(string reason)
        {
            return new PaymentProcessResult
            {
                IsSuccess = false,
                FailureReason = reason
            };
        }
    }
}
