using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types

namespace CloudHomework.Models
{
public class UserEntity : TableEntity
        {
            public UserEntity(string partition, string username, string password)
            {
                this.PartitionKey = partition;
                this.RowKey = username;
                this.Password = password;
            }

            public UserEntity() { }

            public string Email { get; set; }

            public string Password { get; set; }
        }
}