using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Shared
{
    public class OrderCreatedMessage
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderCreatedMessageDetail> OrderDetails { get; set; } = new List<OrderCreatedMessageDetail>();
    }

    public class OrderCreatedMessageDetail
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
