using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Shared
{
    public class PaymentInsertDto
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
