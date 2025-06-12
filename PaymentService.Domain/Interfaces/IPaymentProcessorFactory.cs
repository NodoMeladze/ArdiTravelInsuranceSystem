using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Interfaces
{
    public interface IPaymentProcessorFactory
    {
        IPaymentProcessor CreateProcessor(PaymentMethod method);
        IPaymentProcessor CreateProcessor(string methodName);
        IEnumerable<PaymentMethod> GetSupportedMethods();
    }
}
