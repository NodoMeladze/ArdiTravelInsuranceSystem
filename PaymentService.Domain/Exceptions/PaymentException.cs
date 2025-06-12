namespace PaymentService.Domain.Exceptions
{
    public class PaymentException : Exception
    {
        public PaymentException(string message) : base(message) { }
        public PaymentException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PaymentProcessingException(string message) : PaymentException(message)
    {
    }

    public class PaymentNotFoundException(Guid paymentId) : PaymentException($"Payment with ID {paymentId} was not found")
    {
    }
}
