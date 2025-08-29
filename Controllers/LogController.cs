using AzureStorageWebApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageWebApplication.Controllers
{
    public class LogController : Controller
    {
        private readonly AzureStorageService _storage;

        public LogController(AzureStorageService storage)
        {
            _storage = storage;
        }

        // GET: Log
        public async Task<IActionResult> Index()
        {
            var logs = await _storage.GetAllLogsAsync();
            return View(logs);
        }

        // POST: Log/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            await _storage.ClearLogsAsync();
            TempData["Message"] = "Logs cleared successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
