using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class PaymentMean
    {
        public string ID { get; set; }
        public string AccountHolderName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsDefault { get; set; }
        public string IsExisting { get; set; }
        public string Type { get; set; }
        public CreditCard CreditCard { get; set; }
        public Address BillingAddress { get; set; }
    }
}
