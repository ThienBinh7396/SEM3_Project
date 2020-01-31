using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RechargeWeb.Models
{
    public class HistoryWithDetails
    {
        public history h { get; set; }
        public List<history_detail> history_Details { get; set; }
    }
}