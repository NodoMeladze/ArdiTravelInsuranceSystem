namespace PolicyService.Domain.DTOs
{
    public class PaymentResponseDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        // Helper properties
        public bool IsCompleted => Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        public bool IsFailed => Status.Equals("Failed", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => Status.Equals("Pending", StringComparison.OrdinalIgnoreCase);
    }
}
