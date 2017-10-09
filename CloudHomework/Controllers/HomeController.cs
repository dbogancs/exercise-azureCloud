using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using ServiceBusDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using CloudHomework.Models;
using System.Collections;

namespace AzureLab07ServiceBus.Controllers
{
    public class HomeController : Controller
    {
        private const string commentContainer = "comments";
        private const string userTable = "homework";
        private const string userTablePartition = "users";
        private const string logTable = "logs";
        private const string logTablePartition = "logs";
        private const string connectionString = "Endpoint=sb://labor5-h6k1xw.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=EqnXsoUivRxcz0zkEEikn1sSOEEXPYcB1bvqLs6k+TI=";
        private readonly NamespaceManager namespaceManager;

        public HomeController()
        {
            namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
        }

        public ActionResult Index()
        {
            var queues = namespaceManager.GetQueues();
            var result = new List<string>();
            foreach (var item in queues)
            {
                result.Add(item.Path);
            }

            //queues.Select(q => q.Path).ToList();

            return View(result);
        }

        public long MessageCount(string queueName)
        {
            var description = namespaceManager.GetQueue(queueName);
            if(description == null)
            {
                return 0;
            }
            return description.MessageCount;
            
            //return dexcription?.MessageCount ?? 0; 
        }

        public void TableFunc()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            
            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("users");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            // Create a new customer entity.
            UserEntity customer1 = new UserEntity("Homework", "asd", "asd@asd.hu");
            customer1.Email = "asd@asd";
            customer1.Password = "123";

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(customer1);

            // Execute the insert operation.
            table.Execute(insertOperation);
        }

        public void BlobFunc()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("myblob");

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = System.IO.File.OpenRead(@"path\myfile"))
            {
                blockBlob.BeginUploadText(null, null, null);
                blockBlob.BeginDownloadText(null, null);
                blockBlob.UploadFromStream(fileStream);
            }
        }
        

        public JsonResult DoUser(string username, string password) // TABLE User LOGIN (READ CREATE)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference(userTable);

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();


            TableOperation retrieveOperation = TableOperation.Retrieve<UserEntity>(userTablePartition,username);
            
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
            {
                UserEntity u = (UserEntity)retrievedResult.Result;
                if ((u).Password.Equals(password))
                    return this.Json(new {
                        username = username,
                        email = u.Email
                    }, JsonRequestBehavior.AllowGet);
                else
                    return this.Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                UserEntity u = new UserEntity(logTablePartition, username, password);

                // Create the TableOperation object that inserts the customer entity.
                TableOperation insertOperation = TableOperation.Insert(u);

                // Execute the insert operation.
                table.Execute(insertOperation);

                return this.Json(new { result = username, email = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateUser(string username, string email) // TABLE User UPDATE
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference(userTable);

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            TableOperation retrieveOperation = TableOperation.Retrieve<UserEntity>(userTablePartition, username);

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
            {
                UserEntity u = (UserEntity)retrievedResult.Result;
                u.Email = email;
                TableOperation updateOperation = TableOperation.Replace(u);
                
                // Execute the operation.
                table.Execute(updateOperation);

                return this.Json(new { result = username }, JsonRequestBehavior.AllowGet);
            }

            return this.Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserComment(string username) // BLOB Comment READ
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(commentContainer);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(username);
            string text = "";
            try
            {
                text = blockBlob.DownloadText();
            }catch(Exception e)
            {

            }
            if (!text.Equals(""))
            {
                return this.Json(new { username = username, comment = text}, JsonRequestBehavior.AllowGet);
            }

            return this.Json(new { username = username, comment = "" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetUserComment(string username, string comment) // BLOB Comment CREATE UPDATE
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(commentContainer);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(username);

            blockBlob.UploadText(comment);
            
            return this.Json(new { username = username}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRecentLogs(string queuename) // TABLE Log READ
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference(logTable);

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            // Define the query, and select only the Email property.
            TableQuery<DynamicTableEntity> projectionQuery = new TableQuery<DynamicTableEntity>().Select(new string[] { "date", "username", "content", "isPush" });

            // Define an entity resolver to work with the entity after retrieval.
            EntityResolver<string[]> resolver = (pk, rk, ts, props, etag) => 
                    (props.ContainsKey("date")
                    && props.ContainsKey("username")
                    && props.ContainsKey("content") 
                    && props.ContainsKey("isPush") ) ? new string[] {
                    props["date"].DateTime.ToString(),
                    props["username"].StringValue,
                    props["content"].StringValue,
                    props["isPush"].BooleanValue.ToString()} : null;

            List<string[]> list = new List<string[]>();
            foreach (string[] projectedData in table.ExecuteQuery(projectionQuery, resolver, null, null))
            {
                list.Add(projectedData.ToArray());
            }

            return this.Json(new { list = list }, JsonRequestBehavior.AllowGet);

        }

        public void SaveNewPushLog(PushMessage msg) // TABLE Log CREATE
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference(logTable);

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();
            
            TableOperation insertOperation = TableOperation.Insert(msg);

            // Execute the retrieve operation.
            table.Execute(insertOperation);
            
        }

        public void SaveNewPopLog(PopMessage msg) // TABLE Log CREATE
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference(logTable);

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();
            
            TableOperation insertOperation = TableOperation.Insert(msg);

            // Execute the retrieve operation.
            table.Execute(insertOperation);
            
        }

        [HttpPost]
        public void SendMessage(string queueName, string messageBody, string username)
        {
            var client = QueueClient.CreateFromConnectionString(connectionString, queueName);
            var message = new CustomMessage()
            {
                Date = DateTime.Now,
                Body = messageBody
            };
            using(var bm = new BrokeredMessage(message))
            {
                bm.Properties["urgent"] = 1;
                bm.Properties["Priority"] = "High";
                client.Send(bm);
            }

            PushMessage pm = new PushMessage(logTablePartition);
            pm.date = DateTime.Now;
            pm.content = messageBody;
            pm.isPush = true;
            pm.username = username;
            pm.contentLength = messageBody.Length;
            SaveNewPushLog(pm);

        }

        [HttpGet]
        public JsonResult RetrieveMessage(string queueName, string username)
        {
            QueueClient queueClient = QueueClient.CreateFromConnectionString(connectionString, queueName, ReceiveMode.PeekLock);
            BrokeredMessage receivedMessage = queueClient.Receive(new TimeSpan(0, 0, 30));

            if (receivedMessage == null)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var receivedCustomMessage = receivedMessage.GetBody<CustomMessage>();

            var brokeredMsgProperties = new Dictionary<string, object>();
            brokeredMsgProperties.Add("Size", receivedMessage.Size);
            brokeredMsgProperties.Add("MessageId", receivedMessage.MessageId.Substring(0, 15) + "...");
            brokeredMsgProperties.Add("TimeToLive", receivedMessage.TimeToLive.TotalSeconds);
            brokeredMsgProperties.Add("EnqueuedTimeUtc", receivedMessage.EnqueuedTimeUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            brokeredMsgProperties.Add("ExpiresAtUtc", receivedMessage.ExpiresAtUtc.ToString("yyyy-MM-dd HH:mm:ss"));

            var messageInfo = new
            {
                Label = receivedMessage.Label,
                Date = receivedCustomMessage.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                Message = receivedCustomMessage.Body,
                Properties = receivedMessage.Properties.ToArray(),
                BrokeredMsgProperties = brokeredMsgProperties.ToArray()
            };

            PopMessage pm = new PopMessage(logTablePartition);
            pm.date = DateTime.Now;
            pm.content = receivedCustomMessage.Body;
            pm.isPush = false;
            pm.username = username;
            SaveNewPopLog(pm);

            receivedMessage.Complete();
            return this.Json(messageInfo, JsonRequestBehavior.AllowGet);
        }

    }
}