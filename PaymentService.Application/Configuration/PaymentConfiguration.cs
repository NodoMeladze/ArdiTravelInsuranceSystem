using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Configuration
{
    public class PaymentConfiguration(IConfiguration configuration) : IPaymentConfiguration
    {
        private readonly IConfiguration _configuration = configuration;

        public string DatabaseConnectionString =>
            _configuration.GetConnectionString("PaymentDatabase") ?? "Data Source=payments.db";

        public TimeSpan PaymentTimeout =>
            TimeSpan.FromSeconds(_configuration.GetValue("Payment:TimeoutSeconds", 30));

        public int MaxRetryAttempts =>
            _configuration.GetValue("Payment:MaxRetryAttempts", 3);

        public bool EnableIdempotency =>
            _configuration.GetValue("Payment:EnableIdempotency", true);

        public Dictionary<string, string> PaymentGatewaySettings =>
            _configuration.GetSection("Payment:GatewaySettings")
                         .GetChildren()
                         .ToDictionary(x => x.Key, x => x.Value ?? string.Empty);
    }
}
