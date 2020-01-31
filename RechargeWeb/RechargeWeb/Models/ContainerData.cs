using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RechargeWeb.Models
{
    public class ContainerData
    {
        public List<mobile_network> mobile_Networks { get; set; }
        public List<service> services { get; set; }
        public List<type_service> type_Services { get; set; }
        public List<HistoryWithDetails> historyWithDetails { get; set; }
        public float number { get; set; }
    }
}