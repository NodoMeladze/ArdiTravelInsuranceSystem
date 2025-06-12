using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolicyService.Application.Interfaces;
using PolicyService.Domain.DTOs;
using PolicyService.Domain.Exceptions;
using System.Text.Json;

namespace PolicyService.Infrastructure.ExternalServices
{
    public class PaymentServiceClient : IPaymentServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public PaymentServiceClient(HttpClient httpClient, ILogger<PaymentServiceClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var baseUrl = configuration["PaymentService:BaseUrl"] ?? "http://localhost:5000";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var timeout = configuration.GetValue("PaymentService:TimeoutSeconds", 30);
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
        }

        public async Task<PaymentResponseDto?> GetPaymentStatusAsync(Guid paymentId)
        {
            _logger.LogDebug("Checking payment status for PaymentId: {PaymentId}", paymentId);

            try
            {
                var response = await _httpClient.GetAsync($"/api/payments/{paymentId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var paymentDto = JsonSerializer.Deserialize<PaymentServiceResponse>(responseJson, _jsonOptions);

                    if (paymentDto == null) return null;

                    return new PaymentResponseDto
                    {
                        Id = paymentDto.Id,
                        Amount = paymentDto.Amount,
                        Currency = paymentDto.Currency,
                        Status = paymentDto.Status.ToString(),
                        TransactionId = paymentDto.TransactionId,
                        FailureReason = paymentDto.FailureReason,
                        CreatedAt = paymentDto.CreatedAt,
                        ProcessedAt = paymentDto.ProcessedAt
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    _logger.LogError("Failed to get payment status. Status: {StatusCode}", response.StatusCode);
                    throw new PaymentFailedException($"Failed to get payment status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when getting payment status");
                throw new PaymentFailedException($"Payment service communication error: {ex.Message}");
            }
        }

        public async Task<bool> IsPaymentCompletedAsync(Guid paymentId)
        {
            var payment = await GetPaymentStatusAsync(paymentId);
            return payment?.IsCompleted ?? false;
        }

        // Internal DTO for deserializing PaymentService responses
        private class PaymentServiceResponse
        {
            public Guid Id { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; } = string.Empty;
            public PaymentServiceStatus Status { get; set; }
            public string? TransactionId { get; set; }
            public string? FailureReason { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ProcessedAt { get; set; }
        }

        private enum PaymentServiceStatus
        {
            Pending = 1,
            Completed = 2,
            Failed = 3,
            Refunded = 4
        }
    }
}