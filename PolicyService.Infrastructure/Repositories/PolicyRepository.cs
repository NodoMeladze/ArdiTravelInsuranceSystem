using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Interfaces;
using PolicyService.Infrastructure.Data;

namespace PolicyService.Infrastructure.Repositories
{
    public class PolicyRepository(PolicyDbContext context, ILogger<PolicyRepository> logger) : IPolicyRepository
    {
        private readonly PolicyDbContext _context = context;
        private readonly ILogger<PolicyRepository> _logger = logger;

        public async Task<Policy?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Retrieving policy with ID: {PolicyId}", id);

            return await _context.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Policy> AddAsync(Policy policy)
        {
            _logger.LogDebug("Adding new policy with ID: {PolicyId}", policy.Id);

            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Policy {PolicyId} added successfully", policy.Id);
            return policy;
        }

        public async Task<Policy> UpdateAsync(Policy policy)
        {
            _logger.LogDebug("Updating policy with ID: {PolicyId}", policy.Id);

            _context.Entry(policy).State = EntityState.Modified;
            policy.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Policy {PolicyId} updated successfully", policy.Id);
            return policy;
        }

        public async Task<Policy?> GetByPaymentIdAsync(Guid paymentId)
        {
            _logger.LogDebug("Retrieving policy with PaymentId: {PaymentId}", paymentId);
            return await _context.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }
    }
}