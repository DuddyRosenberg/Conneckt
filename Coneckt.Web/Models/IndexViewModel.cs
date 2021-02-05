using Conneckt.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Coneckt.Web.Models
{
    public class IndexViewModel
    {
        public List<ProductID> ProductIDs { get; set; }
        public dynamic PaymentSources { get; set; }
    }
}