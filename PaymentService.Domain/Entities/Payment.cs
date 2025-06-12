using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        public string? TransactionId { get; set; }
        public string? IdempotencyKey { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        // Explicit payment method fields for storage
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? PayPalEmail { get; set; }

        // Business logic methods
        public void MarkAsCompleted(string transactionId)
        {
            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            ProcessedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string reason)
        {
            Status = PaymentStatus.Failed;
            FailureReason = reason;
            ProcessedAt = DateTime.UtcNow;
        }

        public bool IsCompleted => Status == PaymentStatus.Completed;
        public bool IsFailed => Status == PaymentStatus.Failed;
    }
}