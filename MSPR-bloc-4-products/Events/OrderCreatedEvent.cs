using System.Collections.Generic;

namespace MSPR_bloc_4_orders.Events
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public List<ProductOrderItem> Products { get; set; } = new();
    }

    public class ProductOrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
