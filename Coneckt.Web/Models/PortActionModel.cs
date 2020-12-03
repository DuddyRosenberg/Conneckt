using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coneckt.Web.Models
{
    public class PortActionModel : ActivateActionModel
    {
        public string CurrentMIN { get; set; }
        public string CurrentServiceProvider { get; set; }
        public string CurrentAccountNumber { get; set; }
        public string CurrentVKey { get; set; }
    }
}
