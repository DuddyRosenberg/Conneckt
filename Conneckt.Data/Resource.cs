using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class Resource
    {
        public Location Location { get; set; }
        public Association Association { get; set; }
        public Specification ResourceSpecification { get; set; }
        public PhysicalResource PhysicalResource { get; set; }
        public List<SupportingResource> SupportingLogicalResources { get; set; }
    }
}
