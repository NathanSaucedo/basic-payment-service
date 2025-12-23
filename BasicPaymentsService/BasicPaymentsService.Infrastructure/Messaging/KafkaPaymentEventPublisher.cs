using BasicPaymentsService.Application.DTOs;
using BasicPaymentsService.Application.Messaging;
using Confluent.Kafka;
using System.Text.Json;

namespace BasicPaymentsService.Infrastructure.Messaging
{
    public class KafkaPaymentEventPublisher(IProducer<string, string> producer, string topic) : IPaymentEventPublisher
    {
        private readonly IProducer<string, string> _producer = producer;
        private readonly string _topic = topic;

        public async Task PublishPaymentRegisteredEventAsync(PaymentResponseDto payment)
        {
            var message = new Message<string, string>
            {
                Key = payment.PaymentId.ToString(),
                Value = JsonSerializer.Serialize(payment)
            };
            await _producer.ProduceAsync(_topic, message);
        }
    }
}
