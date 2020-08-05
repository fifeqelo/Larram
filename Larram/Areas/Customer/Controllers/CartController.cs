using Larram.DataAccess.Data;
using Larram.DataAccess.Repository.IRepository;
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
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

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
            if(item.Quantity >= item.ProductAvailability.Quantity)
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
    }
}
