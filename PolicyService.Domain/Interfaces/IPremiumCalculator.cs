using PolicyService.Domain.Enums;

namespace PolicyService.Domain.Interfaces
{
    public interface IPremiumCalculator
    {
        decimal Calculate(CoverageType coverageType, DateTime startDate, DateTime endDate, string destination);
        decimal GetBasePremium(CoverageType coverageType);
        decimal GetDestinationMultiplier(string destination);
        decimal GetDurationMultiplier(int days);
    }
}
