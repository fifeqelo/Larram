using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
using Larram.Models.ViewModels;
using Larram.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Larram.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderDetailsViewModel orderDetailsViewModel { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int? page, string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<Order> orderList;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderList = await _unitOfWork.Order.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderList = await _unitOfWork.Order.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "inprocess":
                    orderList = orderList.Where(o => o.OrderStatus == SD.StatusInProcess ||
                                                o.OrderStatus == SD.StatusApproved);
                    break;
                case "completed":
                    orderList = orderList.Where(o => o.OrderStatus == SD.StatusInShipped);
                    break;
                case "rejected":
                    orderList = orderList.Where(o => o.OrderStatus == SD.StatusCancelled);
                    break;
                default:
                    break;
            }

            int pageSize = 10;

            return View(PaginatedList<Order>.Create(orderList, page ?? 1, pageSize));
        }

        public async Task<IActionResult> Details(int id)
        {
            orderDetailsViewModel = new OrderDetailsViewModel()
            {
                Order = await _unitOfWork.Order.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = await _unitOfWork.OrderDetails.GetAll(o => o.OrderId == id, includeProperties: "ProductAvailability"),
            };
            foreach(var item in orderDetailsViewModel.OrderDetails)
            {
                orderDetailsViewModel.ProductAvailabilities = await _unitOfWork.ProductAvailability.GetAll(o => o.ProductId == item.ProductAvailability.ProductId, includeProperties: "Product");
            }

            return View(orderDetailsViewModel);
        }

        [Authorize(Roles = SD.Role_Admin+","+SD.Role_Employee)]
        public async Task<IActionResult> StartProcessing(int id)
        {
            Order order = await _unitOfWork.Order.GetFirstOrDefault(u => u.Id == id);
            order.OrderStatus = SD.StatusInProcess;
            await _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            Order order = await _unitOfWork.Order.GetFirstOrDefault(u => u.Id == id);
            order.OrderStatus = SD.StatusCancelled;
            await _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> ShipOrder()
        {
            Order order = await _unitOfWork.Order.GetFirstOrDefault(u => u.Id == orderDetailsViewModel.Order.Id);
            order.TrackingNumber = orderDetailsViewModel.Order.TrackingNumber;
            order.Carrier = orderDetailsViewModel.Order.Carrier;
            order.ShippingDate = DateTime.Now;
            order.OrderStatus = SD.StatusInShipped;
            await _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
