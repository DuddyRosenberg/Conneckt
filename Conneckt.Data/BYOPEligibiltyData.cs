using System;
using System.Collections.Generic;

namespace Conneckt.Data
{
    public class BYOPEligibiltyData
    {
        public List<RelatedParty> RelatedParties { get; set; }
        public Location Location { get; set; }
        public List<ServiceCategory> ServiceCategory { get; set; }
        public Specification ServiceSpecification { get; set; }
        public Service Service { get; set; }
        public List<RelatedResource> RelatedResources { get; set; }
    }
}
