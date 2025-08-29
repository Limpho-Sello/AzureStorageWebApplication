using System.Diagnostics;
using AzureStorageWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using AzureStorageWebApplication.Services;

namespace AzureStorageWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly AzureStorageService _storageService;

        public HomeController(AzureStorageService storageService)
        {
            _storageService = storageService;
        }

        public IActionResult Index()
        {
            ViewData["Message"] = "Welcome to ABC Retail!";
            return View();
        }

     
    }
}