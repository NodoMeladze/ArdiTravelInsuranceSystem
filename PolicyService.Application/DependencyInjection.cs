using Microsoft.Extensions.DependencyInjection;
using PolicyService.Application.Interfaces;
using PolicyService.Application.Mappers;
using PolicyService.Application.Services;
using PolicyService.Domain.Interfaces;

namespace PolicyService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPolicyService, Services.PolicyService>();
            services.AddScoped<IPolicyMapper, PolicyMapper>();
            services.AddScoped<ICircuitBreaker, CircuitBreakerService>();

            return services;
        }
    }
}