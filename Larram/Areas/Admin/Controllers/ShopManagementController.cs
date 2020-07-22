using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Larram.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Larram.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShopManagementController : Controller
    {
        private readonly ILogger<ShopManagementController> _logger;

        public ShopManagementController(ILogger<ShopManagementController> logger)
        {
            _logger = logger;
        }
        public IActionResult Menu()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
