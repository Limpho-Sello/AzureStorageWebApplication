using Azure.Storage.Blobs;
using AzureStorageWebApplication.Models;
using AzureStorageWebApplication.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace AzureStorageWebApplication.Controllers
{
    public class ProductController : Controller
    {
        private readonly AzureStorageService _storage;

        public ProductController(AzureStorageService storage)
        {
            _storage = storage;
        }
        // GET: Product
        public async Task<IActionResult> Index()
        {
            var products = await _storage.GetAllProductsAsync();
            return View(products);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
             public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    string imageUrl = await _storage.UploadImageAsync(
                        product.ImageFile,
                        product.ImageFile.FileName,
                        product.Title
                    );
                    product.ImageUrl = imageUrl;
                }

                await _storage.AddProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _storage.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product product)
        {
            if (id != product.RowKey) return NotFound();

            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    string imageUrl = await _storage.UploadImageAsync(
                    product.ImageFile,
                    product.ImageFile.FileName,
                     product.Title   

                    );
                    product.ImageUrl = imageUrl;
                }

                await _storage.UpdateProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        // GET: Product/Details/rowKey
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            // Fetch the product from the service using RowKey
            var product = await _storage.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Products/Delete
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var product = await _storage.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product); 
        }

        // POST: Products/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            await _storage.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(string rowKey)
        {
            var product = await _storage.GetProductByIdAsync(rowKey);
            if (product == null || string.IsNullOrEmpty(product.ImageUrl))
                return NotFound();

            var httpClient = new HttpClient();
            var stream = await httpClient.GetStreamAsync(product.ImageUrl);
            var fileName = product.ImageUrl.Split('/').Last();

            return File(stream, "application/octet-stream", fileName);
        }

    }
}
