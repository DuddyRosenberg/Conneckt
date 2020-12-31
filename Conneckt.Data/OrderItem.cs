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
        public int? Quantity { get; set; }
        public ProductOffering ProductOffering { get; set; }
        public List<Extension> CharacteristicSpecification { get; set; }
        public OrderPrice OrderItemPrice { get; set; }
    }
}
