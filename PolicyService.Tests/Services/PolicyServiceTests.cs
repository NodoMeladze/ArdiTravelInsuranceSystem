using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PolicyService.Application.Interfaces;
using PolicyService.Domain.DTOs;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Enums;
using PolicyService.Domain.Interfaces;
using PolicyService.Domain.ValueObjects;

namespace PolicyService.Tests.Services
{
    public class PolicyServiceTests
    {
        private readonly Mock<IPolicyRepository> _mockRepository;
        private readonly Mock<IPaymentServiceClient> _mockPaymentClient;
        private readonly Mock<IPremiumCalculator> _mockCalculator;
        private readonly Mock<IPolicyValidator> _mockValidator;
        private readonly Mock<IPolicyMapper> _mockMapper;
        private readonly Mock<ICircuitBreaker> _mockCircuitBreaker;
        private readonly Mock<ILogger<Application.Services.PolicyService>> _mockLogger;
        private readonly Application.Services.PolicyService _policyService;

        public PolicyServiceTests()
        {
            _mockRepository = new Mock<IPolicyRepository>();
            _mockPaymentClient = new Mock<IPaymentServiceClient>();
            _mockCalculator = new Mock<IPremiumCalculator>();
            _mockValidator = new Mock<IPolicyValidator>();
            _mockMapper = new Mock<IPolicyMapper>();
            _mockCircuitBreaker = new Mock<ICircuitBreaker>();
            _mockLogger = new Mock<ILogger<Application.Services.PolicyService>>();

            _policyService = new Application.Services.PolicyService(
                _mockRepository.Object,
                _mockPaymentClient.Object,
                _mockCalculator.Object,
                _mockValidator.Object,
                _mockMapper.Object,
                _mockCircuitBreaker.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreatePolicyAsync_ValidRequest_ReturnsPolicy()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var request = new CreatePolicyDto
            {
                CustomerName = "John Doe",
                CustomerEmail = "john@example.com",
                Destination = "Europe",
                TripStartDate = DateTime.UtcNow.AddDays(30),
                TripEndDate = DateTime.UtcNow.AddDays(37),
                CoverageType = CoverageType.Premium,
                PaymentId = paymentId
            };

            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                Status = PolicyStatus.Active,
                PremiumAmount = 70m,
                PaymentId = paymentId
            };

            var expectedDto = new PolicyDto
            {
                Id = policy.Id,
                Status = PolicyStatus.Active,
                PremiumAmount = 70m,
                PaymentId = paymentId
            };

            var paymentResponse = new PaymentResponseDto
            {
                Id = paymentId,
                Status = "Completed",
                Amount = 70m
            };

            // Setup mocks
            _mockValidator.Setup(v => v.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Success());
            _mockRepository.Setup(r => r.GetByPaymentIdAsync(paymentId))
                .ReturnsAsync((Policy?)null);
            _mockCircuitBreaker.Setup(cb => cb.ExecuteAsync(It.IsAny<Func<Task<PaymentResponseDto?>>>()))
                .ReturnsAsync(paymentResponse);
            _mockCalculator.Setup(c => c.Calculate(It.IsAny<CoverageType>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(70m);
            _mockMapper.Setup(m => m.ToEntity(request)).Returns(policy);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Policy>())).ReturnsAsync(policy);
            _mockMapper.Setup(m => m.ToDto(It.IsAny<Policy>())).Returns(expectedDto);

            // Act
            var result = await _policyService.CreatePolicyAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(PolicyStatus.Active);
            result.PremiumAmount.Should().Be(70m);
            result.PaymentId.Should().Be(paymentId);
        }

        [Fact]
        public async Task GetPolicyAsync_ExistingPolicy_ReturnsPolicy()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy = new Policy
            {
                Id = policyId,
                Status = PolicyStatus.Active,
                PremiumAmount = 100m
            };
            var expectedDto = new PolicyDto
            {
                Id = policyId,
                Status = PolicyStatus.Active,
                PremiumAmount = 100m
            };

            _mockRepository.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _mockMapper.Setup(m => m.ToDto(policy)).Returns(expectedDto);

            // Act
            var result = await _policyService.GetPolicyAsync(policyId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(policyId);
            result.Status.Should().Be(PolicyStatus.Active);
        }

        [Fact]
        public async Task UpdatePolicyStatusAsync_ValidTransition_UpdatesStatus()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy = new Policy
            {
                Id = policyId,
                Status = PolicyStatus.Active,
                Customer = new Customer("John", "john@test.com"),
                TripDetails = new TripDetails("Europe", DateTime.Today.AddDays(1), DateTime.Today.AddDays(8))
            };

            var expectedDto = new PolicyDto
            {
                Id = policyId,
                Status = PolicyStatus.Cancelled
            };

            _mockRepository.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Policy>())).ReturnsAsync(policy);
            _mockMapper.Setup(m => m.ToDto(It.IsAny<Policy>())).Returns(expectedDto);

            // Act
            var result = await _policyService.UpdatePolicyStatusAsync(policyId, PolicyStatus.Cancelled);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(PolicyStatus.Cancelled);
            policy.Status.Should().Be(PolicyStatus.Cancelled);
        }
    }
}