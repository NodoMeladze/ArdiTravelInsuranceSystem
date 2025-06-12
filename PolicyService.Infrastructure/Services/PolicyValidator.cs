using Microsoft.Extensions.Logging;
using PolicyService.Domain.DTOs;
using PolicyService.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using ValidationResult = PolicyService.Domain.DTOs.ValidationResult;

namespace PolicyService.Infrastructure.Services
{
    public class PolicyValidator : IPolicyValidator
    {
        private readonly ILogger<PolicyValidator> _logger;
        private readonly HashSet<string> _validDestinations;
        private readonly EmailAddressAttribute _emailValidator;

        public PolicyValidator(ILogger<PolicyValidator> logger)
        {
            _logger = logger;
            _emailValidator = new EmailAddressAttribute();
            _validDestinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Europe", "France", "Germany", "Italy", "Spain", "UK", "Netherlands",
                "North America", "USA", "Canada", "Mexico",
                "Asia", "Japan", "China", "Thailand", "Singapore", "India",
                "Australia", "New Zealand",
                "South America", "Brazil", "Argentina", "Chile",
                "Africa", "South Africa", "Egypt", "Morocco",
                "Middle East", "UAE", "Turkey", "Israel"
            };
        }

        public async Task<ValidationResult> ValidateAsync(CreatePolicyDto request)
        {
            _logger.LogDebug("Validating policy creation request for customer: {Email}", request.CustomerEmail);

            var result = new ValidationResult();

            ValidateCustomerInfo(request, result);

            ValidateTripDetails(request, result);

            ValidateCoverageType(request, result);

            ValidatePaymentId(request, result);

            await Task.CompletedTask; // Placeholder for async validations (e.g., checking blacklists)

            if (!result.IsValid)
            {
                _logger.LogWarning("Policy validation failed for customer {Email}. Errors: {Errors}",
                    request.CustomerEmail, string.Join(", ", result.Errors));
            }

            return result;
        }

        public async Task<ValidationResult> ValidateUpdateAsync(Guid policyId, UpdatePolicyStatusDto request)
        {
            _logger.LogDebug("Validating policy status update for policy: {PolicyId}", policyId);

            var result = new ValidationResult();

            if (!Enum.IsDefined(request.Status))
            {
                result.AddError("Invalid policy status");
            }

            await Task.CompletedTask;

            return result;
        }

        public bool IsValidDestination(string destination)
        {
            return _validDestinations.Contains(destination) ||
                   _validDestinations.Any(d => destination.Contains(d, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsValidTripDuration(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                return false;

            if (startDate < DateTime.Today)
                return false;

            var duration = (endDate - startDate).Days;
            return duration is >= 1 and <= 365;
        }

        private void ValidateCustomerInfo(CreatePolicyDto request, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
                result.AddError("Customer name is required");
            else if (request.CustomerName.Length < 2)
                result.AddError("Customer name must be at least 2 characters");
            else if (request.CustomerName.Length > 100)
                result.AddError("Customer name cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
                result.AddError("Customer email is required");
            else if (!_emailValidator.IsValid(request.CustomerEmail))
                result.AddError("Invalid email format");
            else if (request.CustomerEmail.Length > 255)
                result.AddError("Email cannot exceed 255 characters");

            if (!string.IsNullOrWhiteSpace(request.CustomerPhoneNumber))
            {
                if (request.CustomerPhoneNumber.Length > 20)
                    result.AddError("Phone number cannot exceed 20 characters");
            }
        }

        private void ValidateTripDetails(CreatePolicyDto request, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(request.Destination))
                result.AddError("Destination is required");
            else if (!IsValidDestination(request.Destination))
                result.AddError($"Destination '{request.Destination}' is not supported");

            if (!IsValidTripDuration(request.TripStartDate, request.TripEndDate))
            {
                if (request.TripStartDate >= request.TripEndDate)
                    result.AddError("Trip end date must be after start date");
                else if (request.TripStartDate < DateTime.Today)
                    result.AddError("Trip start date cannot be in the past");
                else
                    result.AddError("Invalid trip duration (maximum 365 days allowed)");
            }
        }

        private static void ValidateCoverageType(CreatePolicyDto request, ValidationResult result)
        {
            if (!Enum.IsDefined(request.CoverageType))
                result.AddError("Invalid coverage type");
        }

        private static void ValidatePaymentId(CreatePolicyDto request, ValidationResult result)
        {
            if (request.PaymentId == Guid.Empty)
                result.AddError("Valid payment ID is required");
        }
    }
}