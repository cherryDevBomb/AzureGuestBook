using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Azure;

namespace GuestBookData
{
    // https://docs.microsoft.com/en-us/azure/cosmos-db/tutorial-develop-table-dotnet
    // https://docs.microsoft.com/en-us/azure/cosmos-db/create-table-dotnet
    // https://technet2.github.io/Wiki/blogs/windowsazurestorage/windows-azure-storage-client-library-2-0-tables-deep-dive.html
    // ~ 1:50:30
    public class GuestBookDataSource
    {
        private static readonly string _tableName = "GuestBookEntry";

        private static CloudStorageAccount _storageAccount;
        private static CloudTable _table;


        static GuestBookDataSource()
        {
            var storageConnectionString = AppSettings.LoadAppSettings().StorageConnectionString;

            _storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            _table = tableClient.GetTableReference(_tableName);
            _table.CreateIfNotExists();
        }

        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");

                throw;
            }

            return storageAccount;
        }

        public IEnumerable<GuestBookEntry> GetGuestBookEntries()
        {
            var entities = _table.ExecuteQuery(new TableQuery<GuestBookEntry>()).OrderBy(e => e.RowKey).ToList();

            return entities;
        }

        public GuestBookEntry GetGuestBookEntryByPhotoURL(string photoUrl)
        {

            TableQuery<GuestBookEntry> query =
                new TableQuery<GuestBookEntry>().Where(
                    TableQuery.GenerateFilterCondition("PhotoUrl", QueryComparisons.Equal, photoUrl));
            var entities = _table.ExecuteQuery(query);
            var entity = entities.ElementAt(0);
            return entity;
        }

        public void EditGuestBookEntryThumbUrl(string thumbnailUrl, string photoUrl)
        {
            TableQuery<GuestBookEntry> query =
                new TableQuery<GuestBookEntry>().Where(
                    TableQuery.GenerateFilterCondition("PhotoUrl", QueryComparisons.Equal, photoUrl));
            var entities = _table.ExecuteQuery(query);
            var entity = entities.ElementAt(0);
            entity.ThumbnailUrl = thumbnailUrl;
            TableOperation updateOperation = TableOperation.Merge(entity);
            _table.Execute(updateOperation);
        }

        public void AddGuestBookEntry(GuestBookEntry newItem)
        {
            TableOperation tableOperation = TableOperation.Insert(newItem);
            _table.Execute(tableOperation);
        }

        public void UpdateImageThumbnail(GuestBookEntry entry)
        {
            TableOperation updateOperation = TableOperation.Merge(entry);
            _table.Execute(updateOperation);
        }
    }
}
