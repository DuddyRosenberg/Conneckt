using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class Authorization
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public DateTime exp_dateTime { get; set; }
    }
}
