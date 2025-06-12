using PaymentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.Domain.DTOs
{
    public class ProcessPaymentDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "GEL";

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public required string IdempotencyKey { get; set; }

        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? PayPalEmail { get; set; }
    }
}
