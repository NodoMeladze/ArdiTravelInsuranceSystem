using System.Data;
using Dapper;

namespace PaymentService.Infrastructure.Data
{
    public class DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        public async Task InitializeAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            await CreateTablesAsync(connection);
            await CreateIndexesAsync(connection);
        }

        private static async Task CreateTablesAsync(IDbConnection connection)
        {
            const string createPaymentsTable = @"
                CREATE TABLE IF NOT EXISTS Payments (
                    Id TEXT PRIMARY KEY,
                    Amount REAL NOT NULL,
                    Currency TEXT NOT NULL DEFAULT 'GEL',
                    Status INTEGER NOT NULL,
                    Method INTEGER NOT NULL,
                    TransactionId TEXT NULL,
                    IdempotencyKey TEXT NULL,
                    FailureReason TEXT NULL,
                    CardNumber TEXT NULL,
                    CardHolderName TEXT NULL,
                    PayPalEmail TEXT NULL,
                    CreatedAt TEXT NOT NULL,
                    ProcessedAt TEXT NULL
                );";

            await connection.ExecuteAsync(createPaymentsTable);
        }

        private static async Task CreateIndexesAsync(IDbConnection connection)
        {
            const string createIndexes = @"
                CREATE INDEX IF NOT EXISTS IX_Payments_Status ON Payments(Status);
                CREATE INDEX IF NOT EXISTS IX_Payments_CreatedAt ON Payments(CreatedAt);
                CREATE UNIQUE INDEX IF NOT EXISTS IX_Payments_IdempotencyKey ON Payments(IdempotencyKey) WHERE IdempotencyKey IS NOT NULL;
                CREATE INDEX IF NOT EXISTS IX_Payments_TransactionId ON Payments(TransactionId) WHERE TransactionId IS NOT NULL;
            ";

            await connection.ExecuteAsync(createIndexes);
        }
    }
}
