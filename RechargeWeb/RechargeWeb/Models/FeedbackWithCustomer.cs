using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RechargeWeb.Models
{
    public class FeedbackWithCustomer
    {
        public List<feedback> receiveFeedbacks { get; set; }
        public List<feedback> sendFeedbacks { get; set; }
        public List<feedback> listWithSort { get; set; }
        public customer customers { get; set; }
        public employee employees { get; set; }
    }
}