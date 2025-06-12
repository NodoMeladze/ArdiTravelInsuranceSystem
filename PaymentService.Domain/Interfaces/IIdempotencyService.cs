namespace PaymentService.Domain.Interfaces
{
    public interface IIdempotencyService
    {
        Task<bool> IsProcessedAsync(string key);
        Task MarkAsProcessedAsync(string key, Guid paymentId);
    }
}
