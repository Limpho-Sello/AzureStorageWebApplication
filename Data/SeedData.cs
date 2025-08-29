using AzureStorageWebApplication.Models;
using AzureStorageWebApplication.Services;

namespace AzureStorageWebApplication.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(AzureStorageService storageService)
        {
            // ===== Seed Customers =====
         
                var customers = new List<Customer>
            {
                new Customer
                {
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "Kananelo Sello",
                    Email = "kananelo.sello@outlook.com",
                    Phone = "0712245638",
              
                },
                new Customer
                {
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "Rachel Kunutu",
                    Email = "rkunutu@gmail.com",
                    Phone = "0723478965",
             
                },
                new Customer
                {
                    RowKey = Guid.NewGuid().ToString(),
                    Name = "Pulane Ntho",
                    Email = "pulane1ntho@yahoo.com",
                    Phone = "0734567770",
             
                }
                };

                foreach (var customer in customers)
                {
                    try
                    {
                        await storageService.AddCustomerAsync(customer);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding customer {customer.Name}: {ex.Message}");
                    }
                }
            

            // ===== Seed Products =====          
                var products = new List<Product>
                 {
                new Product
                {
                    RowKey = Guid.NewGuid().ToString(),
                    Title = "Women's floral dress",
                    Description = "Comfortable cotton dress in multiple floral designs and colors.",
                    Price = 349.99m,
                    Inventory = 120,
                  
                    ImageUrl = "https://st10490959.blob.core.windows.net/productimages/women's%20floral%20dress.webp"
                },
                new Product
                {
                    RowKey = Guid.NewGuid().ToString(),
                    Title = "Wireless Headphones",
                    Description = "Noise-cancelling wireless earbuds with 12 hours battery life.",
                    Price = 799.50m,
                    Inventory = 200,
                 
                    ImageUrl = "https://st10490959.blob.core.windows.net/productimages/wireless%20headphones.webp"
                },
                new Product
                {
                    RowKey = Guid.NewGuid().ToString(),
                    Title = "Desk Lamp",
                    Description = "Adjustable LED desk lamp with touch control and USB charging port.",
                    Price = 499.00m,
                    Inventory = 75,
                   // CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://st10490959.blob.core.windows.net/productimages/desk%20lamp.webp"
                }
                };
                foreach (var product in products)
                {
                    try
                    {
                        await storageService.AddProductAsync(product);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding product {product.Title}: {ex.Message}");
                    }
                }
            }
        }
    } 

