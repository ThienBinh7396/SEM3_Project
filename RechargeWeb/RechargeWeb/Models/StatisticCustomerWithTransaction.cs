using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RechargeWeb.Models
{
    public class StatisticCustomerWithTransaction
    {
        public customer customers { get; set; }
        public int amount { get; set; }
        public int amount_success { get; set; }
        public float total { get; set; }
    }
}