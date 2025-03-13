using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImageProcessing.Services
{
    public class ConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<ConsumerService> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["KafkaSettings:BootstrapServers"],
                GroupId = "image-processing-group",
                EnableAutoCommit = false,
                SecurityProtocol = SecurityProtocol.SaslSsl, 
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = _configuration["KafkaSettings:SaslUsername"],
                SaslPassword = _configuration["KafkaSettings:SaslPassword"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SessionTimeoutMs = 10000, // 10 seconds
                MaxPollIntervalMs = 300000 // 5 minutes
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_configuration["KafkaSettings:Topic"]);
            Console.WriteLine("Consumer is listening...");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);  


                        var imageInfo = JsonConvert.DeserializeObject<Models.Image>(consumeResult.Message.Value);

                        using var scope = _scopeFactory.CreateScope();
                        var imageService = scope.ServiceProvider.GetRequiredService<IImageProcessingService>();
                        await imageService.ProcessAndStoreVariantsAsync(imageInfo);
                        consumer.Commit(consumeResult);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Consumer operation canceled.");                                                                                                                                                                                                
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing message: {ex.Message}");
                    }
                }
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
