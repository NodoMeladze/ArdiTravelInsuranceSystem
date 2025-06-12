using Microsoft.Extensions.DependencyInjection;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Factories;
using PaymentService.Infrastructure.PaymentProcessors;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Database
            services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();
            services.AddSingleton<DatabaseInitializer>();

            // Repositories
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            // Payment Processors
            services.AddScoped<CreditCardProcessor>();
            services.AddScoped<PayPalProcessor>();

            // Factory
            services.AddScoped<IPaymentProcessorFactory, PaymentProcessorFactory>();

            return services;
        }

        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            await initializer.InitializeAsync();
        }
    }
}
