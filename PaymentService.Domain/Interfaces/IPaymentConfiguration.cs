namespace PaymentService.Domain.Interfaces
{
    public interface IPaymentConfiguration
    {
        string DatabaseConnectionString { get; }
        TimeSpan PaymentTimeout { get; }
        int MaxRetryAttempts { get; }
        bool EnableIdempotency { get; }
        Dictionary<string, string> PaymentGatewaySettings { get; }
    }
}
