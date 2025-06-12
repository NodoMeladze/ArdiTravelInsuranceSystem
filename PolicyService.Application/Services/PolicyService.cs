using Microsoft.Extensions.Logging;
using PolicyService.Application.Interfaces;
using PolicyService.Domain.DTOs;
using PolicyService.Domain.Enums;
using PolicyService.Domain.Exceptions;
using PolicyService.Domain.Interfaces;

namespace PolicyService.Application.Services
{
    public class PolicyService(
        IPolicyRepository policyRepository,
        IPaymentServiceClient paymentServiceClient,
        IPremiumCalculator premiumCalculator,
        IPolicyValidator policyValidator,
        IPolicyMapper policyMapper,
        ICircuitBreaker circuitBreaker,
        ILogger<PolicyService> logger) : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository = policyRepository;
        private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient;
        private readonly IPremiumCalculator _premiumCalculator = premiumCalculator;
        private readonly IPolicyValidator _policyValidator = policyValidator;
        private readonly IPolicyMapper _policyMapper = policyMapper;
        private readonly ICircuitBreaker _circuitBreaker = circuitBreaker;
        private readonly ILogger<PolicyService> _logger = logger;

        public async Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto request)
        {
            _logger.LogInformation("Creating policy for customer: {CustomerEmail}, destination: {Destination}, paymentId: {PaymentId}",
                request.CustomerEmail, request.Destination, request.PaymentId);

            try
            {
                // 1. Validate the policy request
                var validationResult = await _policyValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new PolicyValidationException($"Policy validation failed: {string.Join(", ", validationResult.Errors)}");
                }

                // 2. Check for existing policy with same payment ID (idempotency)
                var existingPolicy = await _policyRepository.GetByPaymentIdAsync(request.PaymentId);
                if (existingPolicy != null)
                {
                    _logger.LogInformation("Returning existing policy {PolicyId} for payment {PaymentId}",
                        existingPolicy.Id, request.PaymentId);
                    return _policyMapper.ToDto(existingPolicy);
                }

                // 3. Verify payment exists and is completed
                var payment = await _circuitBreaker.ExecuteAsync(async () =>
                {
                    return await _paymentServiceClient.GetPaymentStatusAsync(request.PaymentId);
                });

                if (payment == null)
                {
                    throw new PaymentValidationException($"Payment with ID {request.PaymentId} not found");
                }

                if (!payment.IsCompleted)
                {
                    throw new PaymentValidationException($"Payment {request.PaymentId} is not completed. Current status: {payment.Status}");
                }

                // 4. Calculate expected premium and verify against payment amount
                var expectedPremium = _premiumCalculator.Calculate(
                    request.CoverageType,
                    request.TripStartDate,
                    request.TripEndDate,
                    request.Destination);

                _logger.LogInformation("Expected premium: {ExpectedPremium}, Payment amount: {PaymentAmount}",
                    expectedPremium, payment.Amount);

                var tolerance = 1.0m;

                if (payment.Amount < expectedPremium - tolerance)
                {
                    throw new PaymentValidationException(
                        $"Insufficient payment. Required: {expectedPremium} GEL, Paid: {payment.Amount} GEL");
                }

                // 5. Create policy entity
                var policy = _policyMapper.ToEntity(request);
                policy.PremiumAmount = expectedPremium;
                policy.PaymentId = request.PaymentId;
                policy.Status = PolicyStatus.Active; // Payment is already completed
                policy.CreatedAt = DateTime.UtcNow;

                // 6. Save policy
                await _policyRepository.AddAsync(policy);

                _logger.LogInformation("Policy created successfully. PolicyId: {PolicyId}, PaymentId: {PaymentId}",
                    policy.Id, request.PaymentId);

                return _policyMapper.ToDto(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating policy for customer: {CustomerEmail}", request.CustomerEmail);
                throw new PolicyException("An unexpected error occurred while creating the policy");
            }
        }

        public async Task<PolicyDto?> GetPolicyAsync(Guid id)
        {
            _logger.LogDebug("Retrieving policy with ID: {PolicyId}", id);

            var policy = await _policyRepository.GetByIdAsync(id);
            if (policy == null)
            {
                _logger.LogWarning("Policy not found: {PolicyId}", id);
                return null;
            }

            return _policyMapper.ToDto(policy);
        }

        public async Task<PolicyDto> UpdatePolicyStatusAsync(Guid id, PolicyStatus status)
        {
            _logger.LogInformation("Updating policy {PolicyId} status to {Status}", id, status);

            var policy = await _policyRepository.GetByIdAsync(id) ?? throw new PolicyNotFoundException(id);

            try
            {
                policy.UpdateStatus(status);
                await _policyRepository.UpdateAsync(policy);

                _logger.LogInformation("Policy {PolicyId} status updated to {Status}", id, status);
                return _policyMapper.ToDto(policy);
            }
            catch (InvalidOperationException ex)
            {
                throw new PolicyValidationException(ex.Message);
            }
        }

        public async Task<PolicyQuoteDto> GetQuote(PolicyQuoteRequestDto request)
        {
            _logger.LogInformation("Calculating quote for {CoverageType} coverage to {Destination}",
                request.CoverageType, request.Destination);

            if (request.TripStartDate >= request.TripEndDate)
                throw new PolicyValidationException("Trip end date must be after start date");

            if (request.TripStartDate < DateTime.Today)
                throw new PolicyValidationException("Trip start date cannot be in the past");

            var premium = _premiumCalculator.Calculate(
                request.CoverageType,
                request.TripStartDate,
                request.TripEndDate,
                request.Destination);

            //simulated async validations for future implementations
            await Task.CompletedTask;

            return new PolicyQuoteDto
            {
                Premium = premium,
                Currency = "GEL",
                ValidUntil = DateTime.UtcNow.AddHours(24),
                Destination = request.Destination,
                CoverageType = request.CoverageType.ToString(),
                TripDurationDays = (request.TripEndDate - request.TripStartDate).Days
            };
        }

        public async Task<PolicyDto?> GetPolicyByPaymentIdAsync(Guid paymentId)
        {
            _logger.LogDebug("Retrieving policy with PaymentId: {PaymentId}", paymentId);

            var policy = await _policyRepository.GetByPaymentIdAsync(paymentId);
            if (policy == null)
            {
                _logger.LogDebug("No policy found for PaymentId: {PaymentId}", paymentId);
                return null;
            }

            return _policyMapper.ToDto(policy);
        }
    }
}