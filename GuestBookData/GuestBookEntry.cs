using Microsoft.Azure.Cosmos.Table;
using System;

namespace GuestBookData
{
    public class GuestBookEntry : TableEntity
    {
        public GuestBookEntry()
        {
            PartitionKey = DateTime.UtcNow.ToString("MMdyyyy");

            RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
        }

        public string Message { get; set; }

        public string GuestName { get; set; }

        public string PhotoUrl { get; set; }

        public string ThumbnailUrl { get; set; }
    }
}
