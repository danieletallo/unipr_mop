using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Repository.Model
{
    public class ItemHistory
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int SupplierId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
