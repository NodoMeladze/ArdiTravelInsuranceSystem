using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Services;
using PaymentService.Domain.DTOs;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockRepository;
        private readonly Mock<IPaymentProcessorFactory> _mockProcessorFactory;
        private readonly Mock<IPaymentMapper> _mockMapper;
        private readonly Mock<IIdempotencyService> _mockIdempotencyService;
        private readonly Mock<ILogger<Application.Services.PaymentService>> _mockLogger;
        private readonly Application.Services.PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _mockRepository = new Mock<IPaymentRepository>();
            _mockProcessorFactory = new Mock<IPaymentProcessorFactory>();
            _mockMapper = new Mock<IPaymentMapper>();
            _mockIdempotencyService = new Mock<IIdempotencyService>();
            _mockLogger = new Mock<ILogger<Application.Services.PaymentService>>();

            // Use concrete validation service for simplicity
            var validationService = new PaymentValidationService();

            _paymentService = new Application.Services.PaymentService(
                _mockRepository.Object,
                _mockProcessorFactory.Object,
                _mockMapper.Object,
                _mockIdempotencyService.Object,
                validationService,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ValidRequest_ReturnsSuccessfulPayment()
        {
            // Arrange
            var request = new ProcessPaymentDto
            {
                Amount = 100m,
                Currency = "GEL",
                PaymentMethod = PaymentMethod.CreditCard,
                IdempotencyKey = "test-key",
                CardNumber = "4111111111111111",
                CardHolderName = "John Doe"
            };

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                Currency = request.Currency,
                Method = request.PaymentMethod,
                Status = PaymentStatus.Completed,
                TransactionId = "TXN123"
            };

            var expectedDto = new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = PaymentStatus.Completed,
                IdempotencyKey = "TXN123"
            };

            var processor = new Mock<IPaymentProcessor>();
            processor.Setup(p => p.ProcessAsync(request))
                .ReturnsAsync(PaymentProcessResult.Success("TXN123"));

            _mockIdempotencyService.Setup(s => s.IsProcessedAsync(request.IdempotencyKey))
                .ReturnsAsync(false);
            _mockMapper.Setup(m => m.ToEntity(request)).Returns(payment);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Payment>())).ReturnsAsync(payment);
            _mockProcessorFactory.Setup(f => f.CreateProcessor(request.PaymentMethod))
                .Returns(processor.Object);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync(payment);
            _mockMapper.Setup(m => m.ToDto(It.IsAny<Payment>())).Returns(expectedDto);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(PaymentStatus.Completed);
            result.IdempotencyKey.Should().Be("TXN123");
        }

        [Fact]
        public async Task GetPaymentAsync_ExistingPayment_ReturnsPayment()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var payment = new Payment { Id = paymentId, Amount = 100m };
            var expectedDto = new PaymentDto { Id = paymentId, Amount = 100m };

            _mockRepository.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);
            _mockMapper.Setup(m => m.ToDto(payment)).Returns(expectedDto);

            // Act
            var result = await _paymentService.GetPaymentAsync(paymentId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(paymentId);
            result.Amount.Should().Be(100m);
        }

        [Fact]
        public async Task GetPaymentAsync_NonExistentPayment_ReturnsNull()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync((Payment?)null);

            // Act
            var result = await _paymentService.GetPaymentAsync(paymentId);

            // Assert
            result.Should().BeNull();
        }
    }
}