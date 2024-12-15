using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Shared
{
    public class ItemUpdateDto
    {
        public decimal? Price { get; set; }
        public int? StockQuantity { get; set; }
    }
}
