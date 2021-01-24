using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public class BulkData
    {
        public int ID { get; set; }
        public BulkAction Action { get; set; }
        public string Zip { get; set; }
        public string Serial { get; set; }
        public string Sim { get; set; }
        public string CurrentMIN { get; set; }
        public string CurrentServiceProvider { get; set; }
        public string CurrentAccountNumber { get; set; }
        public string CurrentVKey { get; set; }
        public bool Done { get; set; }
        public string response { get; set; }
    }
}
