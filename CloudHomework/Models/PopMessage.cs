using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudHomework.Models
{
    public class PopMessage : TableEntity
    {
        public PopMessage(string partition)
        {
            this.PartitionKey = partition;
            this.RowKey = DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }

        public string username { get; set; }
        public DateTime date { get; set; }
        public string content { get; set; }
        public bool isPush { get; set; }
    }
}