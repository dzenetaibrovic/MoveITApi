using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Order
    {
        public int OrderId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Distance { get; set; }
        public decimal LivingArea { get; set; }
        public decimal AtticArea { get; set; }
        public decimal Basement { get; set; }
        public bool Piano { get; set; }
        public decimal Price { get; set; }
        public Status Status { get; set; }
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

    }
}
