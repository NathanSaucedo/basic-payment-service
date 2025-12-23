using BasicPaymentsService.Application.DTOs;
using BasicPaymentsService.Domain.Interfaces;

namespace BasicPaymentsService.Application.UseCases
{
    public class GetPaymentsByCustomerUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentsByCustomerUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }
        public async Task<IEnumerable<PaymentResponseDto>> ExecuteAsync(Guid customerId)
        {
            var payments = await _paymentRepository.GetPaymentsByCustomerIdAsync(customerId);
            return payments.Select(payment => new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                ServiceProvider = payment.ServiceProvider,
                Amount = payment.Amount,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            });
        }
    }
}
