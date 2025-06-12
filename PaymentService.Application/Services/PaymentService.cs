using Microsoft.Extensions.Logging;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.DTOs;
using PaymentService.Domain.Exceptions;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Services
{
    public class PaymentService(
        IPaymentRepository paymentRepository,
        IPaymentProcessorFactory paymentProcessorFactory,
        IPaymentMapper paymentMapper,
        IIdempotencyService idempotencyService,
        PaymentValidationService validationService,
        ILogger<PaymentService> logger) : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IPaymentProcessorFactory _paymentProcessorFactory = paymentProcessorFactory;
        private readonly IPaymentMapper _paymentMapper = paymentMapper;
        private readonly IIdempotencyService _idempotencyService = idempotencyService;
        private readonly PaymentValidationService _validationService = validationService;
        private readonly ILogger<PaymentService> _logger = logger;

        public async Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto request)
        {
            _logger.LogInformation("Processing payment request for amount: {Amount} {Currency} using method: {Method}",
                request.Amount, request.Currency, request.PaymentMethod);

            try
            {
                _validationService.ValidatePaymentRequest(request);


                var existingPayment = await HandleIdempotencyAsync(request.IdempotencyKey);
                if (existingPayment != null)
                {
                    _logger.LogInformation("Returning existing payment for idempotency key: {Key}",
                        request.IdempotencyKey);
                    return existingPayment;
                }
                

                var payment = _paymentMapper.ToEntity(request);
                await _paymentRepository.AddAsync(payment);

                // process
                var processor = _paymentProcessorFactory.CreateProcessor(request.PaymentMethod);
                var result = await processor.ProcessAsync(request);

                // Update payment based on result
                if (result.IsSuccess)
                {
                    payment.MarkAsCompleted(result.TransactionId!);
                    _logger.LogInformation("Payment {PaymentId} completed successfully with transaction ID: {TransactionId}",
                        payment.Id, result.TransactionId);
                }
                else
                {
                    payment.MarkAsFailed(result.FailureReason!);
                    _logger.LogWarning("Payment {PaymentId} failed: {Reason}",
                        payment.Id, result.FailureReason);
                }

                // Update payment in repository
                await _paymentRepository.UpdateAsync(payment);

                if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
                {
                    await _idempotencyService.MarkAsProcessedAsync(request.IdempotencyKey, payment.Id);
                }

                return _paymentMapper.ToDto(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing payment");
                throw new PaymentProcessingException("An unexpected error occurred while processing the payment");
            }
        }

        public async Task<PaymentDto?> GetPaymentAsync(Guid id)
        {
            _logger.LogDebug("Retrieving payment with ID: {PaymentId}", id);

            var payment = await _paymentRepository.GetByIdAsync(id);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", id);
                return null;
            }

            return _paymentMapper.ToDto(payment);
        }

        private async Task<PaymentDto?> HandleIdempotencyAsync(string idempotencyKey)
        {
            var isProcessed = await _idempotencyService.IsProcessedAsync(idempotencyKey);

            if (isProcessed)
            {
                var existingPayment = await _paymentRepository.GetByIdempotencyKeyAsync(idempotencyKey);
                return existingPayment != null ? _paymentMapper.ToDto(existingPayment) : null;
            }

            return null;
        }
    }
}
