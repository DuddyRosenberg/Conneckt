using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class PhysicalResource
    {
        public string ResourceCategory { get; set; }
        public string ResourceSubCategory { get; set; }
        public string SerialNumber { get; set; }
        public List<SupportingResource> supportingResources { get; set; }
    }
}
