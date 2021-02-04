using Conneckt.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coneckt.Web.Models
{
    public class ActivateActionModel
    {
        public string Zip { get; set; }
        public string Serial { get; set; }
        public string Sim { get; set; }
        public string PaymentMeanID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string CVV { get; set; }
        public Address BillingAddress { get; set; }
    }
}