using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PolicyService.Domain.Enums;
using PolicyService.Infrastructure.Services;

namespace PolicyService.Tests.Services
{
    public class PremiumCalculatorTests
    {
        private readonly PremiumCalculator _calculator;

        public PremiumCalculatorTests()
        {
            // Create a simple in-memory configuration
            var configData = new Dictionary<string, string>
            {
                ["Premium:MinimumAmount"] = "25.0",
                ["Premium:MaximumAmount"] = "2000.0",
                ["Premium:DestinationMultipliers:Europe"] = "1.0",
                ["Premium:DestinationMultipliers:North America"] = "1.2",
                ["Premium:DestinationMultipliers:Asia"] = "1.5",
                ["Premium:BasePremiums:Basic"] = "50.0",
                ["Premium:BasePremiums:Premium"] = "100.0"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData!)
                .Build();

            var mockLogger = new Mock<ILogger<PremiumCalculator>>();

            _calculator = new PremiumCalculator(configuration, mockLogger.Object);
        }

        [Theory]
        [InlineData(CoverageType.Basic, "Europe", 7, 35.0)]
        [InlineData(CoverageType.Premium, "Europe", 7, 70.0)]
        [InlineData(CoverageType.Basic, "Asia", 7, 52.5)]
        public void Calculate_ValidInputs_ReturnsExpectedPremium(
            CoverageType coverageType, string destination, int days, decimal expectedPremium)
        {
            // Arrange
            var startDate = DateTime.Today.AddDays(30);
            var endDate = startDate.AddDays(days);

            // Act
            var result = _calculator.Calculate(coverageType, startDate, endDate, destination);

            // Assert
            result.Should().Be(expectedPremium);
        }

        [Fact]
        public void GetBasePremium_BasicCoverage_Returns50()
        {
            // Act & Assert
            _calculator.GetBasePremium(CoverageType.Basic).Should().Be(50.0m);
        }

        [Fact]
        public void GetBasePremium_PremiumCoverage_Returns100()
        {
            // Act & Assert
            _calculator.GetBasePremium(CoverageType.Premium).Should().Be(100.0m);
        }

        [Theory]
        [InlineData("Europe", 1.0)]
        [InlineData("Asia", 1.5)]
        [InlineData("Unknown", 1.0)]
        public void GetDestinationMultiplier_ReturnsCorrectMultiplier(string destination, decimal expectedMultiplier)
        {
            // Act & Assert
            _calculator.GetDestinationMultiplier(destination).Should().Be(expectedMultiplier);
        }
    }
}