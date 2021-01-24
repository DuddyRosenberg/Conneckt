using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class RelatedParty
    {
        public string RoleType { get; set; }
        public Party Party { get; set; }
    }
}
