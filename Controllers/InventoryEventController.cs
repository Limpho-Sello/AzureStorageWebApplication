using AzureStorageWebApplication.Models;
using AzureStorageWebApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageWebApplication.Controllers
{
    public class InventoryEventController : Controller
    {
        private readonly AzureStorageService _storage;

        public InventoryEventController(AzureStorageService storage)
        {
            _storage = storage;
        }

        // Display all inventory events
        public async Task<IActionResult> Index()
        {
            var events = await _storage.PeekInventoryEventsAsync();
            return View(events); // strongly typed to List<InventoryEvent>
        }
        // GET: /InventoryEvent/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /InventoryEvent/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryEvent inventoryEvent)
        {
            if (ModelState.IsValid)
            {
                await _storage.QueueInventoryEventAsync(inventoryEvent);
                return RedirectToAction(nameof(Index));
            }
            return View(inventoryEvent);
        }

        // Add a new inventory event manually
        [HttpPost]
        public async Task<IActionResult> AddEvent(InventoryEvent inventoryEvent)
        {
            if (ModelState.IsValid)
            {
                await _storage.QueueInventoryEventAsync(inventoryEvent);
            }
            return RedirectToAction(nameof(Index));
        }

        // Clear all inventory events
        public async Task<IActionResult> Clear()
        {
            await _storage.ClearInventoryQueueAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
