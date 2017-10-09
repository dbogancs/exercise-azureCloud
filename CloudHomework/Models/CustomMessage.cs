using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceBusDemo.Models
{
    [Serializable]
    public class CustomMessage
    {
        public string Body  { get; set; }
        public DateTime Date { get; set; }
    }
}