using PolicyService.Application.Interfaces;
using PolicyService.Domain.DTOs;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Enums;
using PolicyService.Domain.ValueObjects;

namespace PolicyService.Application.Mappers
{
    public class PolicyMapper : IPolicyMapper
    {
        public PolicyDto ToDto(Policy policy)
        {
            return new PolicyDto
            {
                Id = policy.Id,
                CustomerName = policy.Customer.Name,
                CustomerEmail = policy.Customer.Email,
                CustomerPhoneNumber = policy.Customer.PhoneNumber,
                Destination = policy.TripDetails.Destination,
                TripStartDate = policy.TripDetails.StartDate,
                TripEndDate = policy.TripDetails.EndDate,
                TripDurationDays = policy.TripDetails.DurationInDays,
                CoverageType = policy.CoverageType,
                Status = policy.Status,
                PremiumAmount = policy.PremiumAmount,
                PaymentId = policy.PaymentId,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt
            };
        }

        public Policy ToEntity(CreatePolicyDto dto)
        {
            return new Policy
            {
                Id = Guid.NewGuid(),
                Customer = new Customer(dto.CustomerName, dto.CustomerEmail, dto.CustomerPhoneNumber),
                TripDetails = new TripDetails(dto.Destination, dto.TripStartDate, dto.TripEndDate),
                CoverageType = dto.CoverageType,
                PaymentId = dto.PaymentId,
                Status = PolicyStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}