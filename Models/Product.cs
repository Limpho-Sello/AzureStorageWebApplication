using Azure;
using Azure.Data.Tables;
using System.Runtime.Serialization;

namespace AzureStorageWebApplication.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "PRODUCT";
        public string RowKey { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Inventory { get; set; }
        public string? ImageUrl { get; set; }

        [IgnoreDataMember]
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public IFormFile? ImageFile { get; set; }


        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag
        {
            get; set;
        }
    }
}
