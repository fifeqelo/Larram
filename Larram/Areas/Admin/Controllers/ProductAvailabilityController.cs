using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Larram.DataAccess.Data;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
using Larram.Models.ViewModels;
using Larram.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Larram.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductAvailabilityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public ProductAvailabilityViewModel productAvailabilityViewModel { get; set; }

        public ProductAvailabilityController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index(int? productId,string orderBy, int? page = 1)
        {
            ViewData["QuantitySortParm"] = String.IsNullOrEmpty(orderBy) ? "quantity_desc" : "";
            ViewData["CurrentSort"] = orderBy;
            ViewData["productId"] = productId;
            if(productId == null)
            {
                return NotFound();
            }

            productAvailabilityViewModel = new ProductAvailabilityViewModel()
            {
                Product = await _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId),
                ProductAvailabilities = await _unitOfWork.ProductAvailability.GetAll(u => u.ProductId == productId, includeProperties: "Size"),
            };
            if(productAvailabilityViewModel.Product == null)
            {
                return NotFound();
            }
            switch (orderBy)
            {
                case "quantity_desc":
                    productAvailabilityViewModel.ProductAvailabilities = productAvailabilityViewModel.ProductAvailabilities.OrderByDescending(u => u.Quantity);
                    break;
                default:
                    productAvailabilityViewModel.ProductAvailabilities = productAvailabilityViewModel.ProductAvailabilities.OrderBy(u => u.Quantity);
                    break;
            }
            int pageSize = 3;
            return View(PaginatedList<ProductAvailability>.Create(productAvailabilityViewModel.ProductAvailabilities, page ?? 1, pageSize));
        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var objToDelete = await _unitOfWork.ProductAvailability.GetFirstOrDefault(u => u.Id == id);
            if (objToDelete == null)
            {
                return NotFound();
            }
            return View(objToDelete);
        }

        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var objToDelete = await _unitOfWork.ProductAvailability.Get(id);
            await _unitOfWork.ProductAvailability.Remove(objToDelete);
            await _unitOfWork.Save();
            return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.ProductAvailability.GetAll()) });
        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Upsert(int? id, int? productId)
        {
            productAvailabilityViewModel = new ProductAvailabilityViewModel()
            {
                Product = await _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId),
                SizeList = _unitOfWork.Size.GetAllNotAsync().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                ProductAvailability = new ProductAvailability(),
            };
            if (id == null)
            {
                //new productAvailability
                return View(productAvailabilityViewModel);
            }
            productAvailabilityViewModel.ProductAvailability = await _unitOfWork.ProductAvailability.GetFirstOrDefault(u => u.Id == id);
            if (productAvailabilityViewModel.ProductAvailability == null)
            {
                return NotFound();
            }
            //edit productAvailability
            return View(productAvailabilityViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert()
        {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                if(productAvailabilityViewModel.ProductAvailability.Id == 0)
                                {
                                    await _unitOfWork.ProductAvailability.Add(productAvailabilityViewModel.ProductAvailability);
                                }
                                else
                                {
                                    _unitOfWork.ProductAvailability.Update(productAvailabilityViewModel.ProductAvailability);
                                }
                            }
                            catch (DBConcurrencyException)
                            {
                                if (!ProductAvailabilityExists(productAvailabilityViewModel.ProductAvailability.Id))
                                {
                                    return NotFound();
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            await _unitOfWork.Save();
                            return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.ProductAvailability.GetAll()) });
                        }
                        return Json(new { isValid = false, html = PopupHelper.RenderRazorViewToString(this, "Upsert", productAvailabilityViewModel.ProductAvailability) });
        }
        public bool ProductAvailabilityExists(int id)
        {
            return _context.ProductAvailabilities.Any(u => u.Id == id);
        }
    }
}
