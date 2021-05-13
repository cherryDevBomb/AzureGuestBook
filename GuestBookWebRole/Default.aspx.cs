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

        public void SignButton_Click(object sender, EventArgs e)
        {
            if (this.FileUpload1.HasFile)
            {
                this.InitializeStorage();
                //upload the image to blob storage
                string uniqueBlobName = string.Format("guestbookpics/image_{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(this.FileUpload1.FileName));
                string containerName = "guestbookpics-" + Guid.NewGuid();

                //CloudBlockBlob blob = blobStorage.GetContainerReference("guestbookpics").GetBlockBlobReference(uniqueBlobName);
                BlobContainerClient container = blobServiceClient.CreateBlobContainer(containerName);

                blob.Properties.ContentType = this.FileUpload1.PostedFile.ContentType;
                //   using (FileUpload1.PostedFile.InputStream)
                // {
                //   blob.UploadFromStream(FileUpload1.PostedFile.InputStream);
                //}
                // blob.UploadFromStream(this.FileUpload1.FileContent); //.PostedFile.InputStream);
                //Stream stream = blockBlob.OpenRead();


                //blob.UploadFromByteArray(this.FileUpload1.FileBytes, 0, this.FileUpload1.FileBytes.Length);
                blob.UploadFromStream(this.FileUpload1.PostedFile.InputStream);

                System.Diagnostics.Trace.TraceInformation("Upload image '{0}' to blob storage as '{1}'", this.FileUpload1.FileName, uniqueBlobName);

                //create a new entry in table storage
                GuestBookEntry entry = new GuestBookEntry()
                {
                    GuestName = this.NameTextBox.Text,
                    Message = this.MessageTextBox.Text,
                    PhotoUrl = blob.Uri.ToString(),
                    ThumbnailUrl = blob.Uri.ToString()
                };


                ds.AddGuestBookEntry(entry);
                System.Diagnostics.Trace.TraceInformation("Added entry {0}-{1} in table storage for guest '{2}'", entry.PartitionKey, entry.RowKey, entry.GuestName);


                //queue a message to process the image
                var queue = queueStorage.GetQueueReference("guestthumbs");
                var message = new CloudQueueMessage(string.Format("{0},{1},{2}", blob.Uri.ToString(), entry.PartitionKey, entry.RowKey));
                queue.AddMessage(message);
                System.Diagnostics.Trace.TraceInformation("Queued message to process blob '{0}'", uniqueBlobName);
            }

            this.NameTextBox.Text = "";
            this.MessageTextBox.Text = "";

            this.DataList1.DataSource = ds.GetGuestBookEntries();
            this.DataList1.DataBind();
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            this.DataList1.DataSource = ds.GetGuestBookEntries();
            this.DataList1.DataBind();
        }

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