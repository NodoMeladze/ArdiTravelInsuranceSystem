using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.DTOs;
using PaymentService.Domain.Exceptions;

namespace PaymentService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;
        private readonly ILogger<PaymentsController> _logger = logger;

        [HttpPost]
        [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentDto>> ProcessPayment([FromBody] ProcessPaymentDto request)
        {
            _logger.LogInformation("Processing payment request for amount: {Amount} {Currency}",
                request.Amount, request.Currency);

            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(request);

                _logger.LogInformation("Payment processed successfully. PaymentId: {PaymentId}, Status: {Status}",
                    payment.Id, payment.Status);

                return Ok(payment);
            }
            catch (PaymentProcessingException ex)
            {
                _logger.LogWarning("Payment processing failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid payment request: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPayment([FromRoute] Guid id)
        {
            _logger.LogDebug("Retrieving payment details for ID: {PaymentId}", id);

            var payment = await _paymentService.GetPaymentAsync(id);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", id);
                return NotFound(new { message = $"Payment with ID {id} not found", timestamp = DateTime.UtcNow });
            }

            return Ok(payment);
        }
    }
}