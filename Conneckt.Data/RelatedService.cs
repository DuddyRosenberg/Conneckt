﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class RelatedService
    {
        public string ID { get; set; }
        public string Category { get; set; }
        public bool IsRedeemNow { get; set; }
        public string Name { get; set; }
        public string Subcategory { get; set; }
        public ValidFor ValidFor { get; set; }
    }
}
