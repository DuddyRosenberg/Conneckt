using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
  public  class ProductOffering
    {
        public string ID { get; set; }
        public string Category { get; set; }
        public Specification ProductSpecification { get; set; }
        public List<SupportingResource> SupportingResources { get; set; }
        public List<Extension> CharacteristicSpecification { get; set; }
    }
}
