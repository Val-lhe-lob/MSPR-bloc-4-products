using RabbitMQ.Stream.Client;
using System.Net;
using System.Text;
using System.Text.Json;
using MSPR_bloc_4_products.Models;
using MSPR_bloc_4_orders.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MSPR_bloc_4_products.Data;

namespace MSPR_bloc_4_products.Services
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMqConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private StreamSystem? _system;

        public RabbitMqConsumer(IServiceScopeFactory scopeFactory, ILogger<RabbitMqConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var config = new StreamSystemConfig
                {
                    UserName = "guest",
                    Password = "guest",
                    Endpoints = { new IPEndPoint(Dns.GetHostEntry("rabbitmq").AddressList[0], 5552) }
                };

                _system = await StreamSystem.Create(config);
                var streamName = "order_stream";

                if (!await _system.StreamExists(streamName))
                {
                    await _system.CreateStream(new StreamSpec(streamName));
                    _logger.LogInformation("Stream {StreamName} created.", streamName);
                }

                await _system.CreateRawConsumer(new RawConsumerConfig(streamName)
                {
                    MessageHandler = async (consumer, ctx, message) =>
                    {
                        try
                        {
                            var json = Encoding.UTF8.GetString(message.Data.Contents);
                            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

                            if (orderEvent != null)
                            {
                                using var scope = _scopeFactory.CreateScope();
                                var _context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

                                foreach (var item in orderEvent.Products)
                                {
                                    var product = await _context.Products.FindAsync(item.ProductId);
                                    if (product != null && product.Stock.HasValue)
                                    {
                                        product.Stock -= item.Quantity;
                                    }
                                }
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Order processed and product stocks updated.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing RabbitMQ message.");
                        }
                    }
                });

                _logger.LogInformation("RabbitMQ consumer started on stream {StreamName}.", streamName);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ stream initialization failed. Continuing without consumer.");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_system != null)
            {
                await _system.Close();
                _logger.LogInformation("RabbitMQ StreamSystem closed.");
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
