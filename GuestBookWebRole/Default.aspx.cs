using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using GuestBookData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GuestBookWebRole
{
    public partial class _Default : Page
    {
        private static bool storageInitialized = false;
        private static object gate = new object();
        //private static BlobClient blobStorage;
        private static BlobServiceClient blobServiceClient;
        private static QueueClient queueStorage;

        private static GuestBookDataSource ds = new GuestBookDataSource();

        protected override void OnInit(EventArgs e)
        {
            this.DataList1.DataSource = ds.GetGuestBookEntries();
            this.DataList1.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                this.Timer1.Enabled = true;
            }
        }

        // https://github.com/Azure-Samples/azure-sdk-for-net-storage-blob-upload-download/blob/master/v12/Program.cs
        public void SignButton_Click(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                InitializeStorage();
                
                string containerName = "guestbookpics-" + Guid.NewGuid();
                string uniqueBlobName = string.Format("image_{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(FileUpload1.FileName));

                BlobContainerClient container = blobServiceClient.CreateBlobContainer(containerName);
                BlobClient blob = container.GetBlobClient(uniqueBlobName);

                using (var fileStream = FileUpload1.PostedFile.InputStream)
                {
                    blob.Upload(fileStream);
                }
                System.Diagnostics.Trace.TraceInformation("Upload image '{0}' to blob storage as '{1}'", FileUpload1.FileName, uniqueBlobName);

                GuestBookEntry entry = new GuestBookEntry()
                {
                    GuestName = NameTextBox.Text,
                    Message = MessageTextBox.Text,
                    PhotoUrl = blob.Uri.ToString(),
                    ThumbnailUrl = blob.Uri.ToString()
                };
                ds.AddGuestBookEntry(entry);
                System.Diagnostics.Trace.TraceInformation("Added entry {0}-{1} in table storage for guest '{2}'", entry.PartitionKey, entry.RowKey, entry.GuestName);

                //queue a message to process the image
                //var queue = queueStorage.GetQueueReference("guestthumbs");
                //var message = new CloudQueueMessage(string.Format("{0},{1},{2}", blob.Uri.ToString(), entry.PartitionKey, entry.RowKey));
                //queue.AddMessage(message);
                //System.Diagnostics.Trace.TraceInformation("Queued message to process blob '{0}'", uniqueBlobName);
            }

            NameTextBox.Text = "";
            MessageTextBox.Text = "";

            DataList1.DataSource = ds.GetGuestBookEntries();
            DataList1.DataBind();
        }

        //protected void Timer1_Tick(object sender, EventArgs e)
        //{
        //    this.DataList1.DataSource = ds.GetGuestBookEntries();
        //    this.DataList1.DataBind();
        //}

        private void InitializeStorage()
        {
            if (storageInitialized)
            {
                return;
            }

            lock (gate)
            {
                if (storageInitialized)
                {
                    return;
                }

                try
                {
                    //read account configuration settings
                    var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
                    //create blob container for images
                    blobStorage = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobStorage.GetContainerReference("guestbookpics");

                    container.CreateIfNotExists();

                    //configure container for public access
                    var permissions = container.GetPermissions();
                    permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                    container.SetPermissions(permissions);

                    //create queue to communicate with worker role
                    queueStorage = storageAccount.CreateCloudQueueClient();
                    CloudQueue queue = queueStorage.GetQueueReference("guestthumbs");
                    queue.CreateIfNotExists();
                }
                catch (WebException)
                {
                    throw new WebException("Storage services initialization failure." +
                        "Check your storage acceount configuration settings." +
                        "If running locally, ensure that the Development Storage service is running.");
                }

                storageInitialized = true;
            }
        }
    }
}