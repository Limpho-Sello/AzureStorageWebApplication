using AzureStorageWebApplication.Models;
using AzureStorageWebApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageWebApplication.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AzureStorageService _storage;

        public CustomerController(AzureStorageService storage)
        {
            _storage = storage;
        }

        // GET: /Customer
        public async Task<IActionResult> Index()
        {
            var customers = await _storage.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: /Customer/Details/CUST001
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var customer = await _storage.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // GET: /Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.PartitionKey = "CUSTOMER";
            model.RowKey = Guid.NewGuid().ToString().ToUpper().Substring(0, 8);
         

            await _storage.AddCustomerAsync(model);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Customer/Edit
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var customer = await _storage.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: /Customer/Edit/CUST001
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Customer model)
        {
            if (id != model.RowKey) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            model.PartitionKey = "CUSTOMER";

            await _storage.UpdateCustomerAsync(model);

            return RedirectToAction(nameof(Index));
        }
        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var customer = await _storage.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            await _storage.DeleteCustomerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
