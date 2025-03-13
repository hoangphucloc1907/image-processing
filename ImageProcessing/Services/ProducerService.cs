using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Threading.Tasks;

namespace ImageProcessing.Services
{
    public class ProducerService : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly ILogger<ProducerService> _logger;

        public ProducerService(IConfiguration configuration, ILogger<ProducerService> logger)
        {
            _logger = logger;
            _topic = configuration["KafkaSettings:Topic"] ?? "image-processing";

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["KafkaSettings:BootstrapServers"],
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = configuration["KafkaSettings:SaslUsername"],
                SaslPassword = configuration["KafkaSettings:SaslPassword"]
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task SendMessageAsync(string key, object message)
        {
            try
            {
                var messageValue = JsonSerializer.Serialize(message);
                await _producer.ProduceAsync(_topic, new Message<string, string> { Key = key, Value = messageValue });

                _logger.LogInformation($"Message sent to topic '{_topic}' with key '{key}'.");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError($"Kafka produce error: {ex.Error.Reason}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in Kafka producer: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
            _logger.LogInformation("Kafka producer disposed.");
        }
    }
}
