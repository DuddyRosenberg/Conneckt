using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class Location
    {
        public PostalAddress PostalAddress { get; set; }
        public string AddressType { get; set; }
        public PostalAddress Address { get; set; }
    }
}
