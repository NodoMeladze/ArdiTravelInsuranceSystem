using Dapper;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories
{
    public class PaymentRepository(IDbConnectionFactory connectionFactory, ILogger<PaymentRepository> logger) : IPaymentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
        private readonly ILogger<PaymentRepository> _logger = logger;

        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            const string sql = @"
                SELECT Id, Amount, Currency, Status, Method, TransactionId, 
                    IdempotencyKey, FailureReason, CardNumber, CardHolderName,
                    PayPalEmail, CreatedAt, ProcessedAt
                FROM Payments 
                WHERE Id = @Id";

            using var connection = await _connectionFactory.CreateConnectionAsync();

            var result = await connection.QueryFirstOrDefaultAsync<PaymentDto>(sql, new { Id = id.ToString() });

            return result != null ? MapToEntity(result) : null;
        }

        public async Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey)
        {
            const string sql = @"
                SELECT Id, Amount, Currency, Status, Method, TransactionId, 
                    IdempotencyKey, FailureReason, CardNumber, CardHolderName,
                    PayPalEmail, CreatedAt, ProcessedAt
                FROM Payments 
                WHERE IdempotencyKey = @IdempotencyKey";

            using var connection = await _connectionFactory.CreateConnectionAsync();

            var result = await connection.QueryFirstOrDefaultAsync<PaymentDto>(sql, new { IdempotencyKey = idempotencyKey });

            return result != null ? MapToEntity(result) : null;
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            const string sql = @"
                INSERT INTO Payments (Id, Amount, Currency, Status, Method, TransactionId, 
                    IdempotencyKey, FailureReason, CardNumber, CardHolderName, 
                    PayPalEmail, CreatedAt, ProcessedAt)
                VALUES (@Id, @Amount, @Currency, @Status, @Method, @TransactionId, 
                    @IdempotencyKey, @FailureReason, @CardNumber, @CardHolderName, 
                    @PayPalEmail, @CreatedAt, @ProcessedAt)";

            using var connection = await _connectionFactory.CreateConnectionAsync();

            var parameters = new
            {
                Id = payment.Id.ToString(),
                payment.Amount,
                payment.Currency,
                Status = (int)payment.Status,
                Method = (int)payment.Method,
                payment.TransactionId,
                payment.IdempotencyKey,
                payment.FailureReason,
                payment.CardNumber,
                payment.CardHolderName,
                payment.PayPalEmail,
                CreatedAt = payment.CreatedAt.ToString("O"),
                ProcessedAt = payment.ProcessedAt?.ToString("O")
            };

            await connection.ExecuteAsync(sql, parameters);
            return payment;
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            const string sql = @"
                UPDATE Payments 
                SET Amount = @Amount, Currency = @Currency, Status = @Status, 
                    Method = @Method, TransactionId = @TransactionId, 
                    FailureReason = @FailureReason, CardNumber = @CardNumber,
                    CardHolderName = @CardHolderName, PayPalEmail = @PayPalEmail,
                    ProcessedAt = @ProcessedAt
                WHERE Id = @Id";

            using var connection = await _connectionFactory.CreateConnectionAsync();

            var parameters = new
            {
                Id = payment.Id.ToString(),
                payment.Amount,
                payment.Currency,
                Status = (int)payment.Status,
                Method = (int)payment.Method,
                payment.TransactionId,
                payment.IdempotencyKey,
                payment.FailureReason,
                payment.CardNumber,
                payment.CardHolderName,
                payment.PayPalEmail,
                ProcessedAt = payment.ProcessedAt?.ToString("O")
            };

            var rowsAffected = await connection.ExecuteAsync(sql, parameters);

            if (rowsAffected == 0)
                throw new InvalidOperationException($"Payment {payment.Id} not found for update");

            return payment;
        }

        private class PaymentDto
        {
            public string Id { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Currency { get; set; } = string.Empty;
            public int Status { get; set; }
            public int Method { get; set; }
            public string? TransactionId { get; set; }
            public string? IdempotencyKey { get; set; }
            public string? FailureReason { get; set; }
            public string? CardNumber { get; set; }
            public string? CardHolderName { get; set; }
            public string? PayPalEmail { get; set; }
            public string CreatedAt { get; set; } = string.Empty;
            public string? ProcessedAt { get; set; }
        }

        private static Payment MapToEntity(PaymentDto dto)
        {
            return new Payment
            {
                Id = Guid.Parse(dto.Id),
                Amount = dto.Amount,
                Currency = dto.Currency,
                Status = (PaymentStatus)dto.Status,
                Method = (PaymentMethod)dto.Method,
                TransactionId = dto.TransactionId,
                IdempotencyKey = dto.IdempotencyKey,
                FailureReason = dto.FailureReason,
                CardNumber = dto.CardNumber,
                CardHolderName = dto.CardHolderName,
                PayPalEmail = dto.PayPalEmail,
                CreatedAt = DateTime.Parse(dto.CreatedAt),
                ProcessedAt = dto.ProcessedAt != null ? DateTime.Parse(dto.ProcessedAt) : null
            };
        }
    }
}
