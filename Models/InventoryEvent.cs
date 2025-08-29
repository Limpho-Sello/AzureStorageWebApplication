namespace AzureStorageWebApplication.Models
{
    public class InventoryEvent
    {
        public string Product { get; set; } = string.Empty;   
        public int Quantity { get; set; }                      
        public string Action { get; set; } = string.Empty;    
       
    }
}
