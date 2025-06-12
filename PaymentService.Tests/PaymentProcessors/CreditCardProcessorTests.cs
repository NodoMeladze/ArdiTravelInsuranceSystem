using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.Domain.DTOs;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.PaymentProcessors;

namespace PaymentService.Tests.PaymentProcessors
{
    public class CreditCardProcessorTests
    {
        private readonly Mock<ILogger<CreditCardProcessor>> _mockLogger;
        private readonly CreditCardProcessor _processor;

        public CreditCardProcessorTests()
        {
            _mockLogger = new Mock<ILogger<CreditCardProcessor>>();
            _processor = new CreditCardProcessor(_mockLogger.Object);
        }

        [Fact]
        public void SupportedMethod_ShouldReturnCreditCard()
        {
            // Act & Assert
            _processor.SupportedMethod.Should().Be(PaymentMethod.CreditCard);
        }

        [Fact]
        public void CanProcess_CreditCardMethod_ShouldReturnTrue()
        {
            // Act & Assert
            _processor.CanProcess(PaymentMethod.CreditCard).Should().BeTrue();
        }

        [Fact]
        public void CanProcess_OtherMethod_ShouldReturnFalse()
        {
            // Act & Assert
            _processor.CanProcess(PaymentMethod.PayPal).Should().BeFalse();
        }

        [Fact]
        public async Task ProcessAsync_ValidRequest_ShouldReturnResult()
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

            // Act
            var result = await _processor.ProcessAsync(request);

            // Assert
            result.Should().NotBeNull();
            // Since the processor simulates random success/failure, we just verify structure
            if (result.IsSuccess)
            {
                result.TransactionId.Should().NotBeNullOrEmpty();
                result.FailureReason.Should().BeNull();
            }
            else
            {
                result.FailureReason.Should().NotBeNullOrEmpty();
                result.TransactionId.Should().BeNull();
            }
        }

        [Fact]
        public async Task ProcessAsync_EmptyCardNumber_ShouldReturnFailure()
        {
            // Arrange
            var request = new ProcessPaymentDto
            {
                Amount = 100m,
                Currency = "GEL",
                PaymentMethod = PaymentMethod.CreditCard,
                IdempotencyKey = "test-key",
                CardNumber = "",
                CardHolderName = "John Doe"
            };

            // Act
            var result = await _processor.ProcessAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.FailureReason.Should().Contain("Card number is required");
        }
    }
}