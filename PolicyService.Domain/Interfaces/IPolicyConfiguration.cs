namespace PolicyService.Domain.Interfaces
{
    public interface IPolicyConfiguration
    {
        string PaymentServiceBaseUrl { get; }
        TimeSpan PaymentServiceTimeout { get; }
        int CircuitBreakerThreshold { get; }
        TimeSpan CircuitBreakerTimeout { get; }
        decimal BasePremiumAmount { get; }
        Dictionary<string, decimal> DestinationMultipliers { get; }
    }
}
