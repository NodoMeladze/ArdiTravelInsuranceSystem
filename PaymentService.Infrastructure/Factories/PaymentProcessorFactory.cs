using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.PaymentProcessors;

namespace PaymentService.Infrastructure.Factories
{
    public class PaymentProcessorFactory : IPaymentProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<PaymentMethod, Type> _processorTypes;

        public PaymentProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _processorTypes = new Dictionary<PaymentMethod, Type>
            {
                { PaymentMethod.CreditCard, typeof(CreditCardProcessor) },
                { PaymentMethod.DebitCard, typeof(CreditCardProcessor) },
                { PaymentMethod.PayPal, typeof(PayPalProcessor) }
            };
        }

        public IPaymentProcessor CreateProcessor(PaymentMethod method)
        {
            if (!_processorTypes.TryGetValue(method, out var processorType))
            {
                throw new PaymentProcessingException($"No processor found for payment method: {method}");
            }

            var processor = _serviceProvider.GetService(processorType) as IPaymentProcessor ?? throw new PaymentProcessingException($"Failed to create processor for payment method: {method}");

            return processor;
        }

        public IPaymentProcessor CreateProcessor(string methodName)
        {
            if (!Enum.TryParse<PaymentMethod>(methodName, true, out var method))
            {
                throw new PaymentProcessingException($"Invalid payment method: {methodName}");
            }

            return CreateProcessor(method);
        }

        public IEnumerable<PaymentMethod> GetSupportedMethods()
        {
            return _processorTypes.Keys;
        }
    }
}
