using BasicPaymentsService.Application.DTOs;

namespace BasicPaymentsService.Application.Messaging
{
    public interface IPaymentEventPublisher
    {
        Task PublishPaymentRegisteredEventAsync(PaymentResponseDto paymentResponse);
    }
}
