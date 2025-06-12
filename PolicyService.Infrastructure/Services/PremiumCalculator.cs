using PolicyService.Domain.Interfaces;
using PolicyService.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PolicyService.Infrastructure.Services
{
    public class PremiumCalculator : IPremiumCalculator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PremiumCalculator> _logger;
        private readonly Dictionary<string, decimal> _destinationMultipliers;
        private readonly Dictionary<CoverageType, decimal> _basePremiums;

        public PremiumCalculator(IConfiguration configuration, ILogger<PremiumCalculator> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Load destination multipliers from configuration
            _destinationMultipliers = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { "Europe", _configuration.GetValue<decimal>("Premium:DestinationMultipliers:Europe", 1.0m) },
                { "North America", _configuration.GetValue<decimal>("Premium:DestinationMultipliers:North America", 1.2m) },
                { "Asia", _configuration.GetValue<decimal>("Premium:DestinationMultipliers:Asia", 1.5m) },
                { "Africa", _configuration.GetValue<decimal>("Premium:DestinationMultipliers:Africa", 2.0m) },
                { "South America", _configuration.GetValue<decimal>("Premium:DestinationMultipliers:South America", 1.8m) },
                { "Australia", _configuration.GetValue<decimal>("Premium:DestinationMultipliers:Australia", 1.4m) },
                { "Middle East", _configuration.GetValue<decimal>("Premium:DestinationMultipliers:Middle East", 2.2m) }
            };

            // Load base premiums from configuration
            _basePremiums = new Dictionary<CoverageType, decimal>
            {
                { CoverageType.Basic, _configuration.GetValue<decimal>("Premium:BasePremiums:Basic", 50.0m) },
                { CoverageType.Premium, _configuration.GetValue<decimal>("Premium:BasePremiums:Premium", 100.0m) }
            };
        }

        public decimal Calculate(CoverageType coverageType, DateTime startDate, DateTime endDate, string destination)
        {
            var tripDuration = (endDate - startDate).Days;
            if (tripDuration <= 0)
                throw new ArgumentException("Trip end date must be after start date");

            // Use daily rates instead of base premium
            decimal dailyRate = coverageType switch
            {
                CoverageType.Basic => 5.0m,
                CoverageType.Premium => 10.0m,
                _ => 5.0m
            };

            var destinationMultiplier = GetDestinationMultiplier(destination);
            var calculatedPremium = dailyRate * tripDuration * destinationMultiplier;

            var minAmount = _configuration.GetValue<decimal>("Premium:MinimumAmount", 25.0m);
            var maxAmount = _configuration.GetValue<decimal>("Premium:MaximumAmount", 2000.0m);
            calculatedPremium = Math.Max(minAmount, Math.Min(maxAmount, calculatedPremium));

            return Math.Round(calculatedPremium, 2);
        }

        public decimal GetBasePremium(CoverageType coverageType)
        {
            var basePremium = _basePremiums.GetValueOrDefault(coverageType, 50.0m);
            _logger.LogDebug("Base premium for {CoverageType}: {Premium}", coverageType, basePremium);
            return basePremium;
        }

        public decimal GetDestinationMultiplier(string destination)
        {
            // Check for exact match first
            if (_destinationMultipliers.TryGetValue(destination, out var exactMultiplier))
            {
                _logger.LogDebug("Exact destination multiplier for {Destination}: {Multiplier}", destination, exactMultiplier);
                return exactMultiplier;
            }

            // Check for partial matches (e.g., "France" contains "Europe")
            foreach (var kvp in _destinationMultipliers)
            {
                if (destination.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.Contains(destination, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Partial destination match for {Destination} -> {Region}: {Multiplier}",
                        destination, kvp.Key, kvp.Value);
                    return kvp.Value;
                }
            }

            // Default multiplier if no match found
            const decimal defaultMultiplier = 1.0m;
            _logger.LogDebug("No destination match for {Destination}, using default multiplier: {Multiplier}",
                destination, defaultMultiplier);
            return defaultMultiplier;
        }

        public decimal GetDurationMultiplier(int days)
        {
            var multiplier = days switch
            {
                <= 7 => 1.0m,      // 1 week or less
                <= 14 => 1.2m,     // 2 weeks
                <= 30 => 1.5m,     // 1 month
                <= 60 => 1.8m,     // 2 months
                <= 90 => 2.0m,     // 3 months
                _ => 2.5m          // Longer trips
            };

            _logger.LogDebug("Duration multiplier for {Days} days: {Multiplier}", days, multiplier);
            return multiplier;
        }
    }
}