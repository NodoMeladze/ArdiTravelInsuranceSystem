using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Services
{
    public class IdempotencyService(IPaymentRepository paymentRepository) : IIdempotencyService
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly Dictionary<string, Guid> _processedKeys = [];

        public async Task<bool> IsProcessedAsync(string key)
        {
            if (_processedKeys.ContainsKey(key))
                return true;

            var payment = await _paymentRepository.GetByIdempotencyKeyAsync(key);
            return payment != null;
        }

        public async Task MarkAsProcessedAsync(string key, Guid paymentId)
        {
            _processedKeys[key] = paymentId;
            await Task.CompletedTask; // In real implementation, might persist to cache/database
        }
    }
}
