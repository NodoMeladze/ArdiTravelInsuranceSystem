using PolicyService.Domain.Enums;
using PolicyService.Domain.ValueObjects;

namespace PolicyService.Domain.Entities
{
    public class Policy
    {
        public Guid Id { get; set; }
        public Customer Customer { get; set; } = null!;
        public TripDetails TripDetails { get; set; } = null!;
        public CoverageType CoverageType { get; set; }
        public PolicyStatus Status { get; set; }
        public decimal PremiumAmount { get; set; }
        public Guid PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Business logic methods
        public void UpdateStatus(PolicyStatus newStatus)
        {
            if (CanTransitionTo(newStatus))
            {
                Status = newStatus;
                UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");
            }
        }

        public bool IsActive => Status == PolicyStatus.Active;
        public bool IsCancellable => Status == PolicyStatus.Active || Status == PolicyStatus.Pending;

        private bool CanTransitionTo(PolicyStatus newStatus)
        {
            return Status switch
            {
                PolicyStatus.Pending => newStatus == PolicyStatus.Active || newStatus == PolicyStatus.Cancelled,
                PolicyStatus.Active => newStatus == PolicyStatus.Cancelled,
                PolicyStatus.Cancelled => false,
                _ => false
            };
        }
    }
}
