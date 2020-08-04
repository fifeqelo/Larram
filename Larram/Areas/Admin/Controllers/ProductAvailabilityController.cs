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

        public ProductAvailabilityController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index(string productName, int productId,string orderBy, string search, string currentFilter, int? page)
        {
            ViewBag.CurrentOrderBy = orderBy;
            ViewBag.SortParam = orderBy;
            ViewBag.ProductName = productName;
            ViewBag.ProductId = productId;


            if (search != null)
            {
                page = 1;
            }
            else
            {
                search = currentFilter;
            }

            ViewBag.CurrentFilter = search;

            var allObj = await _unitOfWork.ProductAvailability.GetAll(filter: n => n.ProductId == productId, includeProperties:"Product,Size");
            if (!String.IsNullOrEmpty(search))
            {
                //allObj = await _unitOfWork.ProductAvailability.GetAll(filter: u => u.Name.Contains(search));
            }
           /* switch (orderBy)
            {
                case "name_asc":
                    allObj = allObj.OrderBy(u => u.Name);
                    break;
                case "name_desc":
                    allObj = allObj.OrderByDescending(u => u.Name);
                    break;
            }*/
            int pageSize = 10;
            return View(PaginatedList<ProductAvailability>.Create(allObj, page ?? 1, pageSize));
            //return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "_ViewAll", (PaginatedList<ProductAvailability>.Create(allObj, page ?? 1, pageSize)) });

        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var objDetails = await _unitOfWork.ProductAvailability.GetFirstOrDefault(d => d.Id == id, includeProperties:"Product,Size");
            if(objDetails == null)
            {
                return NotFound();
            }
            return View(objDetails);
        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var objToDelete = await _unitOfWork.ProductAvailability.Get(id);
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
            ViewBag.ProductId = productId;
            ProductAvailabilityViewModel productAvailabilityViewModel = new ProductAvailabilityViewModel()
            {
                ProductAvailability = new ProductAvailability(),
                SizeList = _unitOfWork.Size.GetAllNotAsync().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
        };
            if(id == null)
            {
                //new productAvailability
                return View(productAvailabilityViewModel);
            }
            productAvailabilityViewModel.ProductAvailability = await _unitOfWork.ProductAvailability.Get(id.GetValueOrDefault());
            if(productAvailabilityViewModel == null)
            { 
                return NotFound();
            }
            //edit productAvailability
            return View(productAvailabilityViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert([Bind("ProductAvailability, SizeList")] ProductAvailabilityViewModel productAvailabilityViewModel)
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
