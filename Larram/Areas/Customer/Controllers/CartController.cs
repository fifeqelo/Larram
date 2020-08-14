using Larram.DataAccess.Data;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
using Larram.Models.ViewModels;
using Larram.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Larram.Areas.Admin.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public ShoppingCartViewModel shoppingCartViewModel { get; set; }

        public CartController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartViewModel = new ShoppingCartViewModel()
            {
                Order = new Models.Order(),
                ListCart = await _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ProductAvailability")
            };

            shoppingCartViewModel.Order.OrderTotal = 0;
            shoppingCartViewModel.Order.ApplicationUser = await _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            foreach(var item in shoppingCartViewModel.ListCart)
            {
                shoppingCartViewModel.ProductAvailability = await _unitOfWork.ProductAvailability.GetFirstOrDefault(u => u.ProductId == item.ProductAvailability.ProductId && u.SizeId == item.ProductAvailability.SizeId, includeProperties: "Product,Size");
                item.Price = shoppingCartViewModel.ProductAvailability.Product.Price;
                shoppingCartViewModel.Order.OrderTotal += (item.Price * item.Quantity);
            }

            return View(shoppingCartViewModel);
        }
        public async Task<IActionResult> Plus(int id)
        {
            var item = await _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == id, includeProperties: "ProductAvailability");
            if(item.Quantity >= 10)
            {
                return RedirectToAction(nameof(Index));
            }
            item.Quantity += 1;
            await _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int id)
        {
            var item = await _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == id);
            if (item.Quantity == 1)
            {
                var count = _unitOfWork.ShoppingCart.GetAllNotAsync(u => u.ApplicationUserId == item.ApplicationUserId).ToList().Count();
                await _unitOfWork.ShoppingCart.Remove(id);
                HttpContext.Session.SetInt32(SD.ShoppingCartSession, count - 1);

            }else
            {
                item.Quantity -= 1;
            }
            await _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int id)
        {
            var item = await _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == id);
            var count = _unitOfWork.ShoppingCart.GetAllNotAsync(u => u.ApplicationUserId == item.ApplicationUserId).ToList().Count();
            await _unitOfWork.ShoppingCart.Remove(id);
            HttpContext.Session.SetInt32(SD.ShoppingCartSession, count - 1);
            await _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartViewModel = new ShoppingCartViewModel()
            {
                Order = new Models.Order(),
                ListCart = await _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ProductAvailability")
            };

            shoppingCartViewModel.Order.ApplicationUser = await _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            foreach (var item in shoppingCartViewModel.ListCart)
            {
                shoppingCartViewModel.ProductAvailability = await _unitOfWork.ProductAvailability.GetFirstOrDefault(u => u.ProductId == item.ProductAvailability.ProductId, includeProperties: "Product");
                item.Price = shoppingCartViewModel.ProductAvailability.Product.Price;
                shoppingCartViewModel.Order.OrderTotal += (item.Price * item.Quantity);
            }

            shoppingCartViewModel.Order.Name = shoppingCartViewModel.Order.ApplicationUser.Name;
            shoppingCartViewModel.Order.PhoneNumber = shoppingCartViewModel.Order.ApplicationUser.PhoneNumber;
            shoppingCartViewModel.Order.PostalCode = shoppingCartViewModel.Order.ApplicationUser.PostalCode;
            shoppingCartViewModel.Order.State = shoppingCartViewModel.Order.ApplicationUser.State;
            shoppingCartViewModel.Order.StreetAdress = shoppingCartViewModel.Order.ApplicationUser.StreetAdress;
            shoppingCartViewModel.Order.City = shoppingCartViewModel.Order.ApplicationUser.City;

            return View(shoppingCartViewModel);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCartViewModel.Order.ApplicationUser = await _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            shoppingCartViewModel.ListCart = await _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value, includeProperties: "ProductAvailability");

            shoppingCartViewModel.Order.OrderStatus = SD.StatusApproved;
            shoppingCartViewModel.Order.ApplicationUserId = claim.Value;
            shoppingCartViewModel.Order.OrderDate = DateTime.Now;
            await _unitOfWork.Order.Add(shoppingCartViewModel.Order);
            await _unitOfWork.Save();

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            foreach(var item in shoppingCartViewModel.ListCart)
            {
                shoppingCartViewModel.ProductAvailability = await _unitOfWork.ProductAvailability.GetFirstOrDefault(u => u.ProductId == item.ProductAvailability.ProductId, includeProperties: "Product");
                item.Price = shoppingCartViewModel.ProductAvailability.Product.Price;
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductAvailabilityId = item.ProductAvailabilityId,
                    OrderId = shoppingCartViewModel.Order.Id,
                    Price = item.Price,
                    Quantity = item.Quantity
                };

                shoppingCartViewModel.Order.OrderTotal += (item.Price * item.Quantity);
                if(shoppingCartViewModel.ProductAvailability.Quantity < item.Quantity)
                {
                    TempData["message"] = "Niestety na obecną chwilę nie mamy na stanie takiej ilości produktu " + shoppingCartViewModel.ProductAvailability.Product.Name;
/*                    ModelState.AddModelError(string.Empty, "Niestety na obecną chwilę nie mamy na stanie takiej ilości produktu "+shoppingCartViewModel.ProductAvailability.Product.Name);
*/                    return RedirectToAction(nameof(Summary));
                } else
                {
                    shoppingCartViewModel.ProductAvailability.Quantity -= item.Quantity;
                }
                await _unitOfWork.OrderDetails.Add(orderDetails);
                await _unitOfWork.Save();

            }
            await _unitOfWork.ShoppingCart.RemoveRange(shoppingCartViewModel.ListCart);
            await _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.ShoppingCartSession, 0);
            return RedirectToAction("OrderConfirmation", "Cart", new { id = shoppingCartViewModel.Order.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
