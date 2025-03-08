using Confluent.Kafka;
using ImageProcessing.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ImageProcessing.Services
{
    public class ConsumerService : BackgroundService
    {
        private readonly ILogger<ConsumerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public ConsumerService(ILogger<ConsumerService> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                SaslUsername = _configuration["Kafka:SaslUsername"],
                SaslPassword = _configuration["Kafka:SaslPassword"],
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                GroupId = "image_processing_group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_configuration["Kafka:Topic"]);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    _logger.LogInformation($"Received message: {consumeResult.Message.Value}");

                    var message = JsonSerializer.Deserialize<ImageProcessingRequest>(consumeResult.Message.Value);
                    if (message != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var imageProcessingService = scope.ServiceProvider.GetRequiredService<IImageProcessingService>();

                        using var imageStream = new MemoryStream(Convert.FromBase64String(message.ImageData));
                        await imageProcessingService.GenerateVariantAsync(imageStream, message.FileName, message.VariantType);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }
    }

    public class ImageProcessingRequest
    {
        public string ImageData { get; set; }
        public string FileName { get; set; }
        public VariantType VariantType { get; set; }
    }
}
