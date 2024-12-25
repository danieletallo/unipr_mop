using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Repository.Model
{
    public class OutboxMessage
    {
        public int Id { get; set; }
        public string? Payload { get; set; }
        public string? Topic { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Processed { get; set; }
    }
}
