using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Shared
{
    public class PaymentStatusChangedMessage
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public List<OrderDetailMessage> OrderDetails { get; set; } = new List<OrderDetailMessage>();
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderDetailMessage
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
