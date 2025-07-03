using RabbitMQ.Stream.Client;
using System.Net;
using System.Text;
using System.Text.Json;
using MSPR_bloc_4_products.Data;
using MSPR_bloc_4_products.Models;
using MSPR_bloc_4_orders.Events;

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
                await system.CreateStream(new StreamSpec(streamName));
            }

            await system.CreateRawConsumer(
                new RawConsumerConfig(streamName)
                {
                    MessageHandler = async (consumer, ctx, message) =>
                    {
                        var json = Encoding.UTF8.GetString(message.Data.Contents);
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
                        }
                    }
                });
        }
    }
}
