using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Configuration;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Mappers;
using PaymentService.Application.Services;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPaymentService, Services.PaymentService>();
            services.AddScoped<IIdempotencyService, IdempotencyService>();
            services.AddScoped<PaymentValidationService>();
            services.AddScoped<IPaymentMapper, PaymentMapper>();
            services.AddScoped<IPaymentConfiguration, PaymentConfiguration>();

            return services;
        }
    }
}
