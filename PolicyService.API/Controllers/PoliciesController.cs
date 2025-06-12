using Microsoft.AspNetCore.Mvc;
using PolicyService.Application.Interfaces;
using PolicyService.Domain.DTOs;
using PolicyService.Domain.Exceptions;

namespace PolicyService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PoliciesController(IPolicyService policyService, ILogger<PoliciesController> logger) : ControllerBase
    {
        private readonly IPolicyService _policyService = policyService;
        private readonly ILogger<PoliciesController> _logger = logger;

        [HttpPost("quote")]
        [ProducesResponseType(typeof(PolicyQuoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PolicyQuoteDto>> GetQuote([FromBody] PolicyQuoteRequestDto request)
        {
            _logger.LogInformation("Getting quote for coverage: {CoverageType}, destination: {Destination}",
                request.CoverageType, request.Destination);

            try
            {
                var quote = await _policyService.GetQuote(request);
                return Ok(quote);
            }
            catch (PolicyValidationException ex)
            {
                _logger.LogWarning("Quote validation failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PolicyDto>> CreatePolicy([FromBody] CreatePolicyDto request)
        {
            _logger.LogInformation("Creating policy for customer: {CustomerEmail}", request.CustomerEmail);

            try
            {
                // Check if policy already exists for this payment
                var existingPolicy = await _policyService.GetPolicyByPaymentIdAsync(request.PaymentId);
                if (existingPolicy != null)
                {
                    _logger.LogInformation("Policy already exists for payment {PaymentId}, returning existing policy", request.PaymentId);
                    return Ok(existingPolicy); // 200 instead of 201
                }

                var policy = await _policyService.CreatePolicyAsync(request);
                _logger.LogInformation("Policy created successfully. PolicyId: {PolicyId}", policy.Id);
                return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, policy); // 201 for new
            }
            catch (PolicyValidationException ex)
            {
                _logger.LogWarning("Policy validation failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (PaymentValidationException ex)
            {
                _logger.LogWarning("Payment validation failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PolicyDto>> GetPolicy([FromRoute] Guid id)
        {
            _logger.LogDebug("Retrieving policy: {PolicyId}", id);

            var policy = await _policyService.GetPolicyAsync(id);
            if (policy == null)
            {
                _logger.LogWarning("Policy not found: {PolicyId}", id);
                return NotFound(new { message = $"Policy with ID {id} not found", timestamp = DateTime.UtcNow });
            }

            return Ok(policy);
        }

        [HttpPut("{id:guid}/status")]
        [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PolicyDto>> UpdatePolicyStatus(
            [FromRoute] Guid id,
            [FromBody] UpdatePolicyStatusDto request)
        {
            _logger.LogInformation("Updating policy {PolicyId} status to {Status}", id, request.Status);

            try
            {
                var policy = await _policyService.UpdatePolicyStatusAsync(id, request.Status);
                return Ok(policy);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound(new { message = $"Policy with ID {id} not found", timestamp = DateTime.UtcNow });
            }
            catch (PolicyValidationException ex)
            {
                return BadRequest(new { message = ex.Message, timestamp = DateTime.UtcNow });
            }
        }
    }
}