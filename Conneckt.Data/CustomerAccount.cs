using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class CustomerAccount
    {
        public string Action { get; set; }
        public List<CustomerProduct> CustomerProducts { get; set; }
        public string Brand { get; set; }
        public List<Extension> CustomerAccountExtension { get; set; }
    }
}
