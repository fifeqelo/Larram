using System;
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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Larram.Utility;
using System.Web;

namespace Larram.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public ProductDisplayViewModel productDisplayViewModel { get; set; }
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAllNotAsync(u => u.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(SD.ShoppingCartSession, count);
            }

            IEnumerable<Product> productList = await _unitOfWork.Product.GetAll();
            productDisplayViewModel = new ProductDisplayViewModel()
            {
                MenProducts = productList.OrderByDescending(u => u.CreatedDate).Where(u => u.Gender.ToString() == "Mężczyzna").Take(4),
                WomenProducts = productList.OrderByDescending(u => u.CreatedDate).Where(u => u.Gender.ToString() == "Kobieta").Take(4),

            };

            return View(productDisplayViewModel);
        }

        public async Task<IActionResult> ShowProducts(string gender, string type, bool discount, string sort)
        {
            ViewData["type"] = type;
            ViewData["gender"] = gender;
            ViewData["discount"] = discount;
            IEnumerable<Product> productList = await _unitOfWork.Product.GetAll(includeProperties: "Category");
            IEnumerable<Product> filteredList = productList
            .GroupBy(u => u.Name)
            .Select(u => u.First());
            if (gender == "mezczyzna")
            {
                filteredList = filteredList.Where(u => u.Gender.ToString().Equals("Mężczyzna"));
            } else
            {
                if(gender == "kobieta")
                {
                    filteredList = filteredList.Where(u => u.Gender.Equals(1));
                } else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            foreach(var item in filteredList)
            {
                filteredList = filteredList.Where(u => u.Category.Name.ToLower().Equals(type));
                if (discount)
                {
                     filteredList = filteredList.Where(u => u.DiscountPrice > 0);
                }
            }
            switch (sort)
            {
                case "priceasc":
                        filteredList = filteredList.OrderBy(u => u.DiscountPrice);
                    break;
                case "pricedesc":
                        filteredList = filteredList.OrderByDescending(u => u.DiscountPrice);
                    break;
                case "newest":
                    filteredList = filteredList.OrderByDescending(u => u.CreatedDate);
                    break;
                default:
                    return View(filteredList);
            }
            return View(filteredList);
        }

        public async Task<IActionResult> ProductDetails(int? id)
        {
            if (id == null) return NotFound();
            string productName = _context.Products.Where(u => u.Id == id).Select(u => u.Name).SingleOrDefault();
            ProductDetailsViewModel productAvailabilityViewModel = new ProductDetailsViewModel() {
                ProductAvailabilities = await _unitOfWork.ProductAvailability.GetAll(filter: u => u.ProductId == id, includeProperties: "Size"),
                Products = await _unitOfWork.Product.GetAll(filter: u => u.Name == productName, includeProperties: "Color"),
                Product = await _unitOfWork.Product.Get(id),
        };
            if (productAvailabilityViewModel.Product == null) return NotFound();
            return View(productAvailabilityViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ProductDetails(int Size, int id, ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                var productId = await _unitOfWork.ProductAvailability.GetFirstOrDefault(u => u.ProductId == id && u.SizeId == Size);

                shoppingCart.ProductAvailabilityId = productId.Id;
                shoppingCart.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == shoppingCart.ApplicationUserId
                && u.ProductAvailabilityId == shoppingCart.ProductAvailabilityId);

                if(cartFromDb == null)
                {
                    await _unitOfWork.ShoppingCart.Add(shoppingCart);
                }
                else
                {
                    cartFromDb.Quantity += shoppingCart.Quantity;
                }
                await _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart.GetAllNotAsync(u => u.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count();

                HttpContext.Session.SetInt32(SD.ShoppingCartSession, count);

                return RedirectToAction(nameof(ShowProducts));
            }
            return RedirectToAction(nameof(ShowProducts));
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
