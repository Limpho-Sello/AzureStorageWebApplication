using AzureStorageWebApplication.Models;
using AzureStorageWebApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageWebApplication.Controllers
{
    public class QueueController : Controller
    {
        private readonly AzureStorageService _storage;

        public QueueController(AzureStorageService storage)
        {
            _storage = storage;
        }

        // Dashboard showing both orders and inventory events
        public async Task<IActionResult> Index()
        {
            var orders = await _storage.GetAllOrdersAsync(); // Returns List<Order>
            var inventory = await _storage.GetAllInventoryEventsAsync(); // Returns List<InventoryEvent>

            var viewModel = new QueueDashboardViewModel
            {
                Orders = orders,
                InventoryEvents = inventory
            };

            return View(viewModel);
        }

        // Clear a queue
        public async Task<IActionResult> ClearQueue(string queueName)
        {
            await _storage.ClearQueueAsync(queueName);
            return RedirectToAction(nameof(Index));
        }

        // Peek messages from any queue
        public async Task<IActionResult> PeekQueue(string queueName)
        {
            var messages = await _storage.GetQueueMessagesAsync(queueName);
            ViewBag.QueueName = queueName;
            return View(messages);
        }
    }

    // ViewModel to combine orders and inventory events
    public class QueueDashboardViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<InventoryEvent> InventoryEvents { get; set; } = new List<InventoryEvent>();
    }
}
