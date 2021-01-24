using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class Request
    {
        public string ID { get; set; }
        public string ExternalID { get; set; }
        public List<Location> Location { get; set; }
        public List<RelatedParty> RelatedParties { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public OrderPrice OrderPrice { get; set; }
        public BillingAccount BillingAccount { get; set; }
    }
}
