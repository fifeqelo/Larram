﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Larram.Models.ViewModels;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Larram.DataAccess.Data;
using Microsoft.EntityFrameworkCore.Internal;
using System.Web.WebPages;

namespace Larram.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ShowProducts()
        {
            IEnumerable<Product> productList = await _unitOfWork.Product.GetAll();
            IEnumerable<Product> filteredList = productList
            .GroupBy(u => u.Name)
            .Select(u => u.First());
            return View(filteredList);
        }

        public async Task<IActionResult> ProductDetails(int? id)
        {
            if (id == null) return NotFound();
            ProductAvailabilityViewModel productAvailabilityViewModel = new ProductAvailabilityViewModel();
            productAvailabilityViewModel.ProductAvailabilities = await _unitOfWork.ProductAvailability.GetAll(filter: u => u.ProductId == id, includeProperties:"Size");
            string productName = _context.Products.Where(u => u.Id == id).Select(u => u.Name).SingleOrDefault();
            productAvailabilityViewModel.Products = await _unitOfWork.Product.GetAll(filter: u => u.Name == productName, includeProperties:"Color");
            productAvailabilityViewModel.Product = await _unitOfWork.Product.Get(id);
            if (productAvailabilityViewModel.Product == null) return NotFound();
            return View(productAvailabilityViewModel);
        }

        public IActionResult Privacy()
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
