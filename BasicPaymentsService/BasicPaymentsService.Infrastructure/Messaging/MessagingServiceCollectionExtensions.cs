using BasicPaymentsService.Application.Messaging;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasicPaymentsService.Infrastructure.Messaging
{
    public static class MessagingServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
            var topic = configuration["Kafka:Topic"] ?? "payments";
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.Leader
            };
            services.AddSingleton<IProducer<string, string>>(_ => new ProducerBuilder<string, string>(producerConfig).Build());
            services.AddScoped<IPaymentEventPublisher>(sp => new KafkaPaymentEventPublisher(sp.GetRequiredService<IProducer<string, string>>(), topic));
            return services;
        }
    }
}
