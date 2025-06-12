using Microsoft.Extensions.DependencyInjection;
using PolicyService.Application.Interfaces;
using PolicyService.Domain.Interfaces;
using PolicyService.Infrastructure.ExternalServices;
using PolicyService.Infrastructure.Repositories;
using PolicyService.Infrastructure.Services;

namespace PolicyService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IPolicyRepository, PolicyRepository>();
            services.AddScoped<IPremiumCalculator, PremiumCalculator>();
            services.AddScoped<IPolicyValidator, PolicyValidator>();
            services.AddScoped<IPaymentServiceClient, PaymentServiceClient>();

            return services;
        }
    }
}