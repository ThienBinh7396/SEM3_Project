using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RechargeWeb.Models
{
    public class Cart
    {
        public float Price { get; set; }
        public int Qty { get; set; }
        public service services { get; set; }
    }
}