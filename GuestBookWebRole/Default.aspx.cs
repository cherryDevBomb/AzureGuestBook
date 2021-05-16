using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using GuestBookData;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GuestBookWebRole
{
    public partial class _Default : Page
    {
        private readonly string containerName = "guestbookpicsblob";

        private static bool _isStorageInitialized = false;
        private static object _lock = new object();

        private static BlobContainerClient _blobContainerClient;
        private static QueueClient _queueClient;
        private static GuestBookDataSource ds = new GuestBookDataSource();

        protected override void OnInit(EventArgs e)
        {
            DataList1.DataSource = ds.GetGuestBookEntries();
            DataList1.DataBind();
        }

        // https://github.com/Azure-Samples/azure-sdk-for-net-storage-blob-upload-download/blob/master/v12/Program.cs
        // https://docs.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues?tabs=dotnet
        public void SignButton_Click(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                InitializeStorage();

                string uniqueBlobName = string.Format("image_{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(FileUpload1.FileName));

                BlobClient blob = _blobContainerClient.GetBlobClient(uniqueBlobName);

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

                // Add message to image processing queue
                if (_queueClient.Exists())
                {
                    var message = uniqueBlobName;
                    _queueClient.SendMessage(message);
                    System.Diagnostics.Trace.TraceInformation("Sent message to process blob '{0}'", uniqueBlobName);
                }
            }

            NameTextBox.Text = "";
            MessageTextBox.Text = "";

            DataList1.DataSource = ds.GetGuestBookEntries();
            DataList1.DataBind();
    
            Timer1.Enabled = true;            
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            DataList1.DataSource = ds.GetGuestBookEntries();
            DataList1.DataBind();

            Timer1.Enabled = false;
        }

        private void InitializeStorage()
        {
            if (_isStorageInitialized)
            {
                return;
            }

            lock (_lock)
            {
                if (_isStorageInitialized)
                {
                    return;
                }

                try
                {
                    // Read account configuration settings
                    var storageConnectionString = AppSettings.LoadAppSettings().StorageConnectionString;

                    // Create blob container for images
                    _blobContainerClient = new BlobContainerClient(storageConnectionString, containerName);
                    _blobContainerClient.CreateIfNotExists();

                    // Configure container for public access
                    _blobContainerClient.SetAccessPolicy(PublicAccessType.Blob);

                    // Create queue to communicate with worker role
                    string queueName = "guestbookthumbnails";
                    _queueClient = new QueueClient(storageConnectionString, queueName);
                    _queueClient.CreateIfNotExists();
                }
                catch (WebException)
                {
                    throw new WebException("Storage services initialization failure." +
                        "Check your storage acceount configuration settings." +
                        "If running locally, ensure that the Development Storage service is running.");
                }

                _isStorageInitialized = true;
            }
        }

        protected void Image_Click1(object sender, ImageClickEventArgs e)
        {
            System.Diagnostics.Trace.TraceInformation("Clicked");
            ImageButton imageBtn = sender as ImageButton;
            ImageFull.ImageUrl = imageBtn.Attributes["FullImageUrl"].ToString();
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "imageModal", "jQuery.noConflict(); $('#imageModal').modal('show');", true);
            upModal.Update();
        }
    }
}