using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using AzureStorageWebApplication.Models;
using System.Text;
using System.Text.Json;

namespace AzureStorageWebApplication.Services
{
    public class AzureStorageService
    {
        private readonly string _connection;
        private readonly string _tableNameCustomers = "Customers";
        private readonly string _tableNameProducts = "Products";
        private readonly string _queueNameOrders = "orders";
        private readonly string _queueNameInventory = "inventory";
        private readonly string _blobContainer = "productimages";
        private readonly string _fileShare = "logs";

        public AzureStorageService(IConfiguration config)
        {
            _connection = config["AzureStorage:ConnectionString"];
        }

        //Customers 
        public async Task AddCustomerAsync(Customer customer)
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameCustomers);
            await table.CreateIfNotExistsAsync();

            var entity = new TableEntity(customer.PartitionKey, customer.RowKey)
            {
                { "Name", customer.Name },
                { "Email", customer.Email },
                { "Phone", customer.Phone ?? "" },

            };

            await table.AddEntityAsync(entity);
            await AppendLogAsync($"Customer added: {customer.RowKey} - {customer.Name}");
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameCustomers);
            await table.CreateIfNotExistsAsync();

            var customers = new List<Customer>();
            await foreach (var entity in table.QueryAsync<TableEntity>(filter: $"PartitionKey eq 'CUSTOMER'"))
            {
                customers.Add(new Customer
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Name = entity.GetString("Name") ?? "",
                    Email = entity.GetString("Email") ?? "",
                    Phone = entity.GetString("Phone"),

                });
            }
            return customers;
        }

        public async Task<Customer?> GetCustomerByIdAsync(string rowKey)
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameCustomers);
            await table.CreateIfNotExistsAsync();

            try
            {
                var entity = await table.GetEntityAsync<TableEntity>("CUSTOMER", rowKey);
                return new Customer
                {
                    PartitionKey = entity.Value.PartitionKey,
                    RowKey = entity.Value.RowKey,
                    Name = entity.Value.GetString("Name") ?? "",
                    Email = entity.Value.GetString("Email") ?? "",
                    Phone = entity.Value.GetString("Phone"),

                };
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameCustomers);
            await table.CreateIfNotExistsAsync();

            var entity = new TableEntity(customer.PartitionKey, customer.RowKey)
            {
                { "Name", customer.Name },
                { "Email", customer.Email },
                { "Phone", customer.Phone ?? "" },

            };

            await table.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
        }

        public async Task DeleteCustomerAsync(string rowKey)
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameCustomers);
            await table.CreateIfNotExistsAsync();

            await table.DeleteEntityAsync("CUSTOMER", rowKey);
        }

        //Products
        public async Task AddProductAsync(Product product)
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameProducts);
            await table.CreateIfNotExistsAsync();

            var entity = new TableEntity(product.PartitionKey, product.RowKey)
            {
                { "Title", product.Title },
                { "Description", product.Description ?? "" },
                { "Price", product.Price },
                { "Inventory", product.Inventory },
                { "ImageUrl", product.ImageUrl ?? "" },

            };

            await table.AddEntityAsync(entity);
            await AppendLogAsync($"Product added: {product.RowKey} - {product.Title}");
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameProducts);
            await table.CreateIfNotExistsAsync();

            var products = new List<Product>();
            await foreach (var entity in table.QueryAsync<TableEntity>(filter: $"PartitionKey eq 'PRODUCT'"))
            {
                products.Add(new Product
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Title = entity.GetString("Title") ?? "",
                    Description = entity.GetString("Description"),
                    Price = Convert.ToDecimal(entity.GetDouble("Price") ?? 0.0),
                    Inventory = entity.GetInt32("Inventory") ?? 0,
                    ImageUrl = entity.GetString("ImageUrl"),

                });
            }
            return products;
        }

        public async Task<Product?> GetProductByIdAsync(string rowKey)
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameProducts);
            await table.CreateIfNotExistsAsync();

            try
            {
                var entity = await table.GetEntityAsync<TableEntity>("PRODUCT", rowKey);
                return new Product
                {
                    PartitionKey = entity.Value.PartitionKey,
                    RowKey = entity.Value.RowKey,
                    Title = entity.Value.GetString("Title") ?? "",
                    Description = entity.Value.GetString("Description"),
                    Price = entity.Value.GetDouble("Price") != null
                        ? Convert.ToDecimal(entity.Value.GetDouble("Price"))
                        : 0m,
                    Inventory = entity.Value.GetInt32("Inventory") ?? 0,
                    ImageUrl = entity.Value.GetString("ImageUrl"),
                };
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameProducts);
            await table.CreateIfNotExistsAsync();

            var entity = new TableEntity(product.PartitionKey, product.RowKey)
            {
                { "Title", product.Title },
                { "Description", product.Description ?? "" },
                { "Price", product.Price },
                { "Inventory", product.Inventory },
                { "ImageUrl", product.ImageUrl ?? "" },

            };

            await table.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
        }

        public async Task DeleteProductAsync(string rowKey)
        {
            var serviceClient = new TableServiceClient(_connection);
            var table = serviceClient.GetTableClient(_tableNameProducts);
            await table.CreateIfNotExistsAsync();

            await table.DeleteEntityAsync("PRODUCT", rowKey);
        }

        //Blob Storage
        public async Task<string> UploadImageAsync(IFormFile file, string blobName, string productTitle)
        {
            var containerClient = new BlobContainerClient(_connection, _blobContainer);
            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            await AppendLogAsync($"Uploaded image: {productTitle} ({blobName})");

            // Inventory queue
            var inventoryEvent = new InventoryEvent
            {
                Product = productTitle,
                Quantity = 0,
                Action = $"Uploaded image for {productTitle}",

            };
            await QueueInventoryEventAsync(inventoryEvent);

            return blobClient.Uri.ToString();
        }


        //Queues
        //orders
        private readonly string _tableNameOrders = "Orders";


        public async Task AddOrderAsync(Order order)
        {
            var tableClient = new TableClient(_connection, _tableNameOrders);
            await tableClient.CreateIfNotExistsAsync();


            var entity = new TableEntity("Order", order.OrderId)
    {
        { "CustomerName", order.CustomerName },
        { "ProductTitle", order.ProductTitle },
        { "Quantity", order.Quantity },
        { "Status", order.Status },
        { "CreatedAt", order.CreatedAt }
    };

            await tableClient.UpsertEntityAsync(entity);

            /*
            await QueueOrderAsync(order);
            */
            await AppendLogAsync($"Order added/updated: {order.OrderId}");
        }

        // Get all Orders
        public async Task<List<Order>> GetOrdersAsync()
        {
            var tableClient = new TableClient(_connection, _tableNameOrders);
            await tableClient.CreateIfNotExistsAsync();

            var orders = new List<Order>();
            var query = tableClient.QueryAsync<TableEntity>(ent => ent.PartitionKey == "Order");

            await foreach (var entity in query)
            {
                orders.Add(new Order
                {
                    OrderId = entity.RowKey,
                    CustomerName = entity.GetString("CustomerName") ?? "",
                    ProductTitle = entity.GetString("ProductTitle") ?? "",
                    Quantity = entity.GetInt32("Quantity") ?? 0,
                    Status = entity.GetString("Status") ?? "Queued",
                    CreatedAt = entity.GetDateTime("CreatedAt") ?? DateTime.UtcNow
                });
            }

            return orders;
        }

        // Get single Order
        public async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            var tableClient = new TableClient(_connection, _tableNameOrders);
            await tableClient.CreateIfNotExistsAsync();

            try
            {
                var entity = await tableClient.GetEntityAsync<TableEntity>("Order", orderId);
                var e = entity.Value;

                return new Order
                {
                    OrderId = e.RowKey,
                    CustomerName = e.GetString("CustomerName") ?? "",
                    ProductTitle = e.GetString("ProductTitle") ?? "",
                    Quantity = e.GetInt32("Quantity") ?? 0,
                    Status = e.GetString("Status") ?? "Queued",
                    CreatedAt = e.GetDateTime("CreatedAt") ?? DateTime.UtcNow
                };
            }
            catch
            {
                return null; // not found
            }
        }

        // Delete an Order
        public async Task DeleteOrderAsync(string orderId)
        {
            var tableClient = new TableClient(_connection, _tableNameOrders);
            await tableClient.CreateIfNotExistsAsync();

            await tableClient.DeleteEntityAsync("Order", orderId);

            await AppendLogAsync($"Order deleted: {orderId}");
        }


        // Queue an Order
        public async Task QueueOrderAsync(Order order)
        {
            var queueClient = new QueueClient(_connection, _queueNameOrders);
            await queueClient.CreateIfNotExistsAsync();

            string msg = JsonSerializer.Serialize(order);
            string base64Msg = Convert.ToBase64String(Encoding.UTF8.GetBytes(msg));
            await queueClient.SendMessageAsync(base64Msg);

            await AppendLogAsync($"Queued order: {order.OrderId}");
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var queueClient = new QueueClient(_connection, _queueNameOrders);
            await queueClient.CreateIfNotExistsAsync();

            var orders = new List<Order>();
            var peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 32);

            foreach (var msg in peekedMessages.Value)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                    var order = JsonSerializer.Deserialize<Order>(json);
                    if (order != null)
                        orders.Add(order);
                }
                catch
                {
                    
                }
            }

            return orders;
        }

        public async Task AddInventoryEventAsync(string product, int quantity, string action)
        {
            var inventoryEvent = new InventoryEvent
            {
                Product = product,
                Quantity = quantity,
                Action = action,

            };

            var queueClient = new QueueClient(_connection, _queueNameInventory);
            await queueClient.CreateIfNotExistsAsync();

            string msg = JsonSerializer.Serialize(inventoryEvent);
            string base64Msg = Convert.ToBase64String(Encoding.UTF8.GetBytes(msg));

            await queueClient.SendMessageAsync(base64Msg);


            await AppendLogAsync($"Queued inventory event: {action} for product {product}");
        }

        public async Task<List<InventoryEvent>> GetInventoryEventsAsync()
        {
            var queueClient = new QueueClient(_connection, _queueNameInventory);
            await queueClient.CreateIfNotExistsAsync();

            var events = new List<InventoryEvent>();
            var peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 32);

            foreach (var msg in peekedMessages.Value)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                    var inventoryEvent = JsonSerializer.Deserialize<InventoryEvent>(json);
                    if (inventoryEvent != null)
                        events.Add(inventoryEvent);
                }
                catch
                {
                    // ignore deserialization errors
                }
            }

            return events;
        }


        public async Task<List<InventoryEvent>> GetAllInventoryEventsAsync()
        {
            var queueClient = new QueueClient(_connection, _queueNameInventory);
            await queueClient.CreateIfNotExistsAsync();

            var events = new List<InventoryEvent>();
            var peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 32);

            foreach (var msg in peekedMessages.Value)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                    var inventoryEvent = JsonSerializer.Deserialize<InventoryEvent>(json);
                    if (inventoryEvent != null) events.Add(inventoryEvent);
                }
                catch
                {
                   
                }
            }

            return events;
        }


        public async Task QueueInventoryEventAsync(InventoryEvent inventoryEvent)
        {
            var queueClient = new QueueClient(_connection, _queueNameInventory);
            await queueClient.CreateIfNotExistsAsync();

            string msg = JsonSerializer.Serialize(inventoryEvent);
            string base64Msg = Convert.ToBase64String(Encoding.UTF8.GetBytes(msg));
            await queueClient.SendMessageAsync(base64Msg);

            await AppendLogAsync($"Queued inventory event: {inventoryEvent.Action} for {inventoryEvent.Product}");
        }

        public async Task<List<InventoryEvent>> PeekInventoryEventsAsync()
        {
            var queueClient = new QueueClient(_connection, _queueNameInventory);
            await queueClient.CreateIfNotExistsAsync();

            var events = new List<InventoryEvent>();
            var peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 32);

            foreach (var msg in peekedMessages.Value)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                    var evt = JsonSerializer.Deserialize<InventoryEvent>(json);
                    if (evt != null) events.Add(evt);
                }
                catch
                {

                    events.Add(new InventoryEvent
                    {
                        Product = "N/A",
                        Quantity = 0,
                        Action = msg.MessageText,

                    });
                }
            }

            return events;
        }

        // Dequeue Inventory Events 
        public async Task<List<InventoryEvent>> DequeueInventoryEventsAsync()
        {
            var queueClient = new QueueClient(_connection, _queueNameInventory);
            await queueClient.CreateIfNotExistsAsync();

            var events = new List<InventoryEvent>();
            var messages = await queueClient.ReceiveMessagesAsync(maxMessages: 32);

            foreach (var msg in messages.Value)
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                var evt = JsonSerializer.Deserialize<InventoryEvent>(json);
                if (evt != null) events.Add(evt);

                // delete after processing
                await queueClient.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
            }

            return events;
        }

        // Clear all Inventory Events
        public async Task ClearInventoryQueueAsync()
        {
            var queueClient = new QueueClient(_connection, _queueNameInventory);
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.ClearMessagesAsync();

            await AppendLogAsync("Cleared inventory queue");
        }


        //Get Messages from any queue 
        public async Task<List<Queue>> GetQueueMessagesAsync(string queueName)
        {
            var queueClient = new QueueClient(_connection, queueName);
            var messagesList = new List<Queue>();

            if (!await queueClient.ExistsAsync()) return messagesList;

            var messages = await queueClient.ReceiveMessagesAsync(maxMessages: 32); 

            foreach (var msg in messages.Value)
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                try
                {
                    var queueItem = JsonSerializer.Deserialize<Queue>(json);
                    if (queueItem != null) messagesList.Add(queueItem);
                }
                catch {  }
            }

            return messagesList;
        }


        public async Task ClearQueueAsync(string queueName)
        {
            var queueClient = new QueueClient(_connection, queueName);
            await queueClient.CreateIfNotExistsAsync();

            await queueClient.ClearMessagesAsync();
            await AppendLogAsync($"Cleared queue: {queueName}");
        }

        //  File Service
        private async Task AppendLogAsync(string text)
        {
            var shareClient = new ShareClient(_connection, _fileShare);
            await shareClient.CreateIfNotExistsAsync();

            var dirClient = shareClient.GetRootDirectoryClient();
            var fileClient = dirClient.GetFileClient("application.log");

            byte[] fileBytes = Array.Empty<byte>();

            try
            {
                var downloadResponse = await fileClient.DownloadAsync();
                using var existingStream = new MemoryStream();
                await downloadResponse.Value.Content.CopyToAsync(existingStream);
                fileBytes = existingStream.ToArray();
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // File does not exist yet
            }

            byte[] newLineBytes = Encoding.UTF8.GetBytes(text + Environment.NewLine);
            byte[] combined = fileBytes.Concat(newLineBytes).ToArray();

            await fileClient.CreateAsync(combined.Length);
            using var uploadStream = new MemoryStream(combined);
            await fileClient.UploadRangeAsync(new Azure.HttpRange(0, combined.Length), uploadStream);
        }

        public async Task<List<string>> GetAllLogsAsync()
        {
            var shareClient = new ShareClient(_connection, _fileShare);
            await shareClient.CreateIfNotExistsAsync();

            var dirClient = shareClient.GetRootDirectoryClient();
            var fileClient = dirClient.GetFileClient("application.log");

            var logs = new List<string>();

            try
            {
                var downloadResponse = await fileClient.DownloadAsync();
                using var reader = new StreamReader(downloadResponse.Value.Content);
                while (!reader.EndOfStream)
                {
                    logs.Add(await reader.ReadLineAsync() ?? "");
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // File does not exist yet
            }

            return logs;
        }

        public async Task ClearLogsAsync()
        {
            var shareClient = new ShareClient(_connection, _fileShare);
            await shareClient.CreateIfNotExistsAsync();

            var dirClient = shareClient.GetRootDirectoryClient();
            var fileClient = dirClient.GetFileClient("application.log");

            try
            {
                await fileClient.DeleteIfExistsAsync();
                await AppendLogAsync("Logs cleared.");
            }
            catch (RequestFailedException)
            {
                // ignore
            }
        }

    } 
}
