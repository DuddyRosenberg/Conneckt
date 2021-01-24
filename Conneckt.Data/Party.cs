using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class Party
    {
        public string PartyID { get; set; }
        public string LanguageAbility { get; set; }
        public List<Extension> PartyExtension { get; set; }
        public Individual Individual { get; set; }
        public Organization Organization { get; set; }
    }
}
