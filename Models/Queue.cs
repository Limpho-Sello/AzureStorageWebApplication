namespace AzureStorageWebApplication.Models
{
    public class Queue
    {
        public string Action { get; set; } = string.Empty;   
        public string Entity { get; set; } = string.Empty;   
        public string Details { get; set; } = string.Empty; 
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

