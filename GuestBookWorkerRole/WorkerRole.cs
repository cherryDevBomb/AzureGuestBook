using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GuestBookData;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues.Models;

namespace GuestBookWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly string picsContainerName = "guestbookpicsblob";
        private readonly string thumbQueueName = "guestbookthumbnails";
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private QueueClient queue;
        private BlobContainerClient container;

        public override void Run()
        {
            Trace.TraceInformation("Listening for queue messages...");

            while (true)
            {
                try
                {
                    // retrieve a new message from the queue
                    QueueMessage msg = queue.ReceiveMessage();
                    if (msg != null)
                    {
                        // parse message retrieved from queue
                        var imageBlobUri = msg.MessageText;
                        Trace.TraceInformation("Processing image in blob '{0}'.", imageBlobUri);
                        string thumbnailName = System.Text.RegularExpressions.Regex.Replace(imageBlobUri, "([^\\.]+)(\\.[^\\.]+)?$", "$1-thumb$2");

                        
                        BlobClient inputBlob = container.GetBlobClient(imageBlobUri);
                        BlobClient outputBlob = container.GetBlobClient(thumbnailName);
                        if (!outputBlob.Exists())
                        {
                            using (Stream input = inputBlob.OpenRead())
                            using (MemoryStream output = new MemoryStream())
                            {

                                this.ProcessImage(input, output);
                                output.Position = 0;

                                outputBlob.Upload(output);
                                // commit the blob and set its properties

                                //outputBlob.Properties.ContentType = "image/jpeg";
                                string thumbnailBlobUri = outputBlob.Uri.ToString();

                                // update the entry in table storage to point to the thumbnail
                                GuestBookDataSource ds = new GuestBookDataSource();
                                GuestBookEntry entry = ds.GetGuestBookEntryByPhotoURL(inputBlob.Uri.ToString());
                                entry.ThumbnailUrl = thumbnailBlobUri;
                                ds.UpdateImageThumbnail(entry);

                                // remove message from queue
                                queue.DeleteMessage(msg.MessageId, msg.PopReceipt);

                                Trace.TraceInformation("Generated thumbnail in blob '{0}'.", thumbnailBlobUri);
                            }
                        }
                        else
                        {
                            // remove message from queue
                            queue.DeleteMessage(msg.MessageId, msg.PopReceipt);
                        }

                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (Azure.RequestFailedException e)
                {
                    Trace.TraceError("Exception when processing queue item. Message: '{0}'", e.Message);
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // get blob container and queue for thumbnails
            var storageConnectionString = AppSettings.LoadAppSettings().StorageConnectionString;
            container = new BlobContainerClient(storageConnectionString, picsContainerName);
            queue = new QueueClient(storageConnectionString, thumbQueueName);


            Trace.TraceInformation("Creating container and queue...");

            bool storageInitialized = false;
            while (!storageInitialized)
            {
                try
                {
                    // create the blob container and allow public access
                    container.CreateIfNotExists();
                    container.SetAccessPolicy(PublicAccessType.Blob);
                    queue.CreateIfNotExists();
                    storageInitialized = true;
                }
                catch (Azure.RequestFailedException e)
                {
                    if (e.Status.Equals(HttpStatusCode.NotFound))
                    {
                        Trace.TraceError("Storage services initialization failure. "
                          + "Check your storage account configuration settings. If running locally, "
                          + "ensure that the Development Storage service is running. Message: '{0}'", e.Message);
                        System.Threading.Thread.Sleep(5000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            bool result = base.OnStart();

            Trace.TraceInformation("GuestBookWorkerRole has been started");

            return result;
        }

        public void ProcessImage(Stream input, Stream output)
        {
            int width;
            int height;
            var originalImage = new Bitmap(input);

            if (originalImage.Width > originalImage.Height)
            {
                width = 128;
                height = 128 * originalImage.Height / originalImage.Width;
            }
            else
            {
                height = 128;
                width = 128 * originalImage.Width / originalImage.Height;
            }

            var thumbnailImage = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(thumbnailImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(originalImage, 0, 0, width, height);
            }

            thumbnailImage.Save(output, ImageFormat.Jpeg);
        }
        public override void OnStop()
        {
            Trace.TraceInformation("GuestBookWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("GuestBookWorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}