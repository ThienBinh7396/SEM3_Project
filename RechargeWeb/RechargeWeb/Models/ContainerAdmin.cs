using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RechargeWeb.Models
{
    public class ContainerAdmin
    {
        public List<history> totalTransactions { get; set; }
        public List<history> successTransactions { get; set; }
        public List<history> pendingTransactions { get; set; }
    }
}