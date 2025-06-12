namespace PolicyService.Domain.Exceptions
{
    public class PolicyException : Exception
    {
        public PolicyException(string message) : base(message) { }
        public PolicyException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PolicyNotFoundException(Guid policyId) : PolicyException($"Policy with ID {policyId} was not found")
    {
    }

    public class PaymentFailedException(string message) : PolicyException(message)
    {
    }

    public class PolicyValidationException(string message) : PolicyException(message)
    {
    }

    public class PaymentValidationException : PolicyException
    {
        public PaymentValidationException(string message) : base(message) { }
        public PaymentValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}