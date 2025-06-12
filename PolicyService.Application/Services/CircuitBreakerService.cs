using PolicyService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace PolicyService.Application.Services
{
    public class CircuitBreakerService(IConfiguration configuration, ILogger<CircuitBreakerService> logger) 
        : ICircuitBreaker
    {
        private readonly ILogger<CircuitBreakerService> _logger = logger;
        private readonly int _failureThreshold = configuration.GetValue("CircuitBreaker:FailureThreshold", 5);
        private readonly TimeSpan _timeout = 
            TimeSpan.FromSeconds(configuration.GetValue("CircuitBreaker:TimeoutSeconds", 60));
        private readonly TimeSpan _retryTimeout = 
            TimeSpan.FromSeconds(configuration.GetValue("CircuitBreaker:RetryTimeoutSeconds", 60));

        private int _failureCount;
        private DateTime _lastFailureTime;
        private CircuitBreakerState _state = CircuitBreakerState.Closed;
        private readonly Lock _lock = new();

        public bool IsOpen => _state == CircuitBreakerState.Open;
        public bool IsHalfOpen => _state == CircuitBreakerState.HalfOpen;
        public bool IsClosed => _state == CircuitBreakerState.Closed;

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            lock (_lock)
            {
                if (_state == CircuitBreakerState.Open)
                {
                    if (DateTime.UtcNow - _lastFailureTime > _retryTimeout)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        _logger.LogInformation("Circuit breaker state changed to HalfOpen");
                    }
                    else
                    {
                        _logger.LogWarning("Circuit breaker is Open, operation blocked");
                        throw new InvalidOperationException("Circuit breaker is open");
                    }
                }
            }

            try
            {
                using var cts = new CancellationTokenSource(_timeout);
                var result = await operation().WaitAsync(cts.Token);

                lock (_lock)
                {
                    if (_state == CircuitBreakerState.HalfOpen)
                    {
                        _state = CircuitBreakerState.Closed;
                        _logger.LogInformation("Circuit breaker state changed to Closed after successful operation");
                    }
                    _failureCount = 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _failureCount++;
                    _lastFailureTime = DateTime.UtcNow;

                    _logger.LogWarning(ex, "Circuit breaker recorded failure {FailureCount}/{Threshold}",
                        _failureCount, _failureThreshold);

                    if (_failureCount >= _failureThreshold)
                    {
                        _state = CircuitBreakerState.Open;
                        _logger.LogError("Circuit breaker state changed to Open due to repeated failures");
                    }
                }

                throw;
            }
        }

        public async Task ExecuteAsync(Func<Task> operation)
        {
            await ExecuteAsync(async () =>
            {
                await operation();
                return Task.CompletedTask;
            });
        }

        private enum CircuitBreakerState
        {
            Closed,
            Open,
            HalfOpen
        }
    }
}