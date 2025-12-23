using BasicPaymentsService.Application.DTOs;
using BasicPaymentsService.Domain.Interfaces;
using BasicPaymentsService.Domain.Entities;
using BasicPaymentsService.Domain.ValueObjects;


namespace BasicPaymentsService.Application.UseCases
{
    public class RegisterPaymentUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public RegisterPaymentUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentResponseDto> ExecuteAsync(RegisterPaymentRequestDto request)
        {
            // Validaciones de monto y moneda
            if (request.Amount > 1500)
                throw new ArgumentException("El monto no puede ser mayor a 1500 Bs.");
            if (string.IsNullOrWhiteSpace(request.ServiceProvider))
                throw new ArgumentException("El proveedor de servicio es requerido.");
            var provider = request.ServiceProvider.Trim();
            if (provider.Length > 100)
                throw new ArgumentException("El proveedor de servicio no debe exceder 100 caracteres.");
            var currencyIn = (request.Currency ?? string.Empty).Trim().ToUpperInvariant();
            var currency = string.IsNullOrEmpty(currencyIn) ? Currency.BOLIVIANOS : currencyIn;
            if (currency == Currency.DOLLARS)
                throw new ArgumentException("Montos en dólares no están permitidos.");
            // Normalizar variantes aceptadas a Bs
            if (currency == "BS" || currency == "BOB" || currency == "BOL")
                currency = Currency.BOLIVIANOS;

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                ServiceProvider = provider,
                Amount = request.Amount,
                Currency = currency,
                Status = PaymentStatus.PENDING,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

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
