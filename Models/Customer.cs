using Azure;
using Azure.Data.Tables;
using System;

namespace AzureStorageWebApplication.Models
{
    public class Customer : ITableEntity
    {
        public string PartitionKey { get; set; } = "CUSTOMER";
        public string RowKey { get; set; } = Guid.NewGuid().ToString(); 
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
       
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

}
