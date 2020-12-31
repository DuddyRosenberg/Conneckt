using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class ServiceData
    {
        public string OrderDate { get; set; }
        public List<RelatedParty> RelatedParties { get; set; }
        public string ExternalID { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public Object Location { get; set; }
    }
}
