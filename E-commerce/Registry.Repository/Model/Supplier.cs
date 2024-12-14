using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registry.Repository.Model
{
    public class Supplier
    {
        public int Id { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
