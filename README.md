# Azure GuestBook
Azure Cloud Service for storing reviews with images. 
> Used as minimal demo for exploring Azure capabilities :wink:

### Features
* After posting a new review, the uploaded image is automatically scaled down (performed by the WorkerRole).
* Original image is available by clicking on its thumbnail.
* The posts are displayed in a chronological reversed order.

### Built with
* Azure Worker Role
* Azure Web Role (ASP.NET WebForm)
* Azure Blob Storage
* Visual Studio 2019 (Azure development + Data storage and processing Workloads)
* Can be locally tested using Azure Storage Emulator

### Deployment
* Use the `StorageConnectionString` from your Azure Account and replace it into `GuestBookWorkerRole\Settings.json` and  `GuestBookWebRole\Settings.json`
* Use Visual Studio in order to `Publish` the Cloud Service