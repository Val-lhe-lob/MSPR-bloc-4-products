using MSPR_bloc_4_orders.Events;
using MSPR_bloc_4_products.Data;
using MSPR_bloc_4_products.Models;
using RabbitMQ.Stream.Client;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MSPR_bloc_4_products.Services
{
    public class RabbitMqConsumer
    {
        private readonly ProductDbContext _context;

        public RabbitMqConsumer(ProductDbContext context)
        {
            _context = context;
            Initialize().GetAwaiter().GetResult();
        }

        private async Task Initialize()
        {
            var config = new StreamSystemConfig
            {
                UserName = "guest",
                Password = "guest",
                Endpoints = { new IPEndPoint(IPAddress.Loopback, 5552) }
            };

            var system = await StreamSystem.Create(config);
            var streamName = "order_stream";

            if (!await system.StreamExists(streamName))
            {
                Console.WriteLine($"[RabbitMqConsumer] Stream '{streamName}' does not exist, creating...");
                await system.CreateStream(new StreamSpec(streamName));
            }

            Console.WriteLine($"[RabbitMqConsumer] Starting Raw Consumer on stream '{streamName}'...");

            await system.CreateRawConsumer(
                new RawConsumerConfig(streamName)
                {
                    MessageHandler = async (consumer, ctx, message) =>
                    {
                        try
                        {
                            var json = Encoding.UTF8.GetString(message.Data.Contents);
                            Console.WriteLine($"[RabbitMqConsumer] Received: {json}");

                            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

                            if (orderEvent != null)
                            {
                                foreach (var item in orderEvent.Products)
                                {
                                    var product = await _context.Products.FindAsync(item.ProductId);
                                    if (product != null && product.Stock.HasValue)
                                    {
                                        product.Stock -= item.Quantity;
                                    }
                                }
                                await _context.SaveChangesAsync();
                                Console.WriteLine($"[RabbitMqConsumer] Stock updated for Order {orderEvent.OrderId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[RabbitMqConsumer] Error processing message: {ex.Message}");
                        }
                    }
                });

            Console.WriteLine("[RabbitMqConsumer] Raw Consumer initialized and listening.");
        }
    }
}
