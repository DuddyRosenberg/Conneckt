using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class Product
    {
        public string ProductCategory { get; set; }
        public Specification ProductSpecification { get; set; }
        public string ProductSerialNumber { get; set; }
        public string ProductStatus { get; set; }
        public string AccountId { get; set; }
        public string SubCategory { get; set; }
        public List<RelatedService> RelatedServices { get; set; }
        public List<SupportingResource> SupportingResources { get; set; }
        public List<Extension> ProductCharacteristics { get; set; }
    }
}
