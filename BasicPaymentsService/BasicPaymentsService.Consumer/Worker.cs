using Confluent.Kafka;

namespace BasicPaymentsService.Consumer
{
    public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092";
            var topic = configuration.GetValue<string>("Kafka:Topic") ?? "payments.basic.services";
            var groupId = configuration.GetValue<string>("Kafka:GroupId") ?? "basic-payments-consumer";

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = consumer.Consume(stoppingToken);
                        logger.LogInformation("Received payment event: key={Key} value={Value}", cr.Message.Key, cr.Message.Value);
                    }
                    catch (ConsumeException ex)
                    {
                        logger.LogError(ex, "Kafka consume error: {Message}", ex.Error.Reason);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Kafka consumer is shutting down.");
            }
            finally
            {
                consumer.Close();
            }
            await Task.CompletedTask;
        }
    }
}
