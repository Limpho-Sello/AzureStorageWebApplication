using AzureStorageWebApplication.Models;
using AzureStorageWebApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageWebApplication.Controllers
{
    public class OrderController : Controller
    {
        private readonly AzureStorageService _storage;

        public OrderController(AzureStorageService storage)
        {
            _storage = storage;
        }

        // GET: /Order
        public async Task<IActionResult> Index()
        {
            var orders = await _storage.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: /Order/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var order = await _storage.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            return View(order);
        }

        // GET: /Order/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                // Save order to storage
                await _storage.AddOrderAsync(order);

                // Queue order (ONLY ONCE)
                await _storage.QueueOrderAsync(order);

                // Redirect to avoid resubmission on refresh
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: /Order/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var order = await _storage.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Order/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _storage.DeleteOrderAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
