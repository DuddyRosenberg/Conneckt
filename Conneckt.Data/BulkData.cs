using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class BulkData
    {
        public int ID { get; set; }
        public BulkAction Action { get; set; }
        public string Zip { get; set; }
        public string Serial { get; set; }
        public string Sim { get; set; }
        public string CurrentMIN { get; set; }
        public string CurrentServiceProvider { get; set; }
        public string CurrentAccountNumber { get; set; }
        public string CurrentVKey { get; set; }
        public bool Done { get; set; }
        public string response { get; set; }
        public string ResourceIdentifier { get; set; }
        public string ResourceType { get; set; }
        public dynamic ResponseObj { get; set; }
        public string PaymentMeanID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string CVV { get; set; }
        public Address BillingAddress { get; set; }
    }
}