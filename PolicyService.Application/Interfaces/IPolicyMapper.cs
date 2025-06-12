using PolicyService.Domain.DTOs;
using PolicyService.Domain.Entities;

namespace PolicyService.Application.Interfaces
{
    public interface IPolicyMapper
    {
        PolicyDto ToDto(Policy policy);
        Policy ToEntity(CreatePolicyDto dto);
    }
}
