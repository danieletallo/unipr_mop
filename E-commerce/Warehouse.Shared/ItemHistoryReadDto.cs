using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Shared
{
    public class ItemHistoryReadDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int SupplierId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
