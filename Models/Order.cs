using System;
using System.ComponentModel.DataAnnotations;
namespace AzureStorageWebApplication.Models
{
    public class Order
    {
        [Key]
        public string OrderId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Title")]
        public string ProductTitle { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Order Status")]
        public string Status { get; set; } = "Queued";

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
