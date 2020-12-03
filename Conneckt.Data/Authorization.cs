using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class Authorization
    {
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
    }
}
