using BasicPaymentsService.Application.DTOs;
using BasicPaymentsService.Domain.Interfaces;

namespace BasicPaymentsService.Application.UseCases
{
    public class GetPaymentByIdUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByIdUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentResponseDto> ExecuteAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId)
                ?? throw new ArgumentException("Pago no encontrado.");

            return new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                ServiceProvider = payment.ServiceProvider,
                Amount = payment.Amount,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };
        }
    }
}
