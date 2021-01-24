using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class AddDeviceData
    {
        public List<RelatedParty> RelatedParties { get; set; }
        public List<CustomerAccount> CustomerAccounts { get; set; }
    }
}
