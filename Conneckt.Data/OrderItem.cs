using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class OrderItem
    {
        public Product Product { get; set; }
        public string ID { get; set; }
        public Location Location { get; set; }
        public string Action { get; set; }
        public List<Extension> OrderItemExtension { get; set; }
    }
}
