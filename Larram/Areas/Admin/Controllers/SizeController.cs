using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Larram.DataAccess.Data;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
using Microsoft.AspNetCore.Mvc;

namespace Larram.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SizeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public SizeController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index(string orderBy, string search, string currentFilter, int? page)
        {
            ViewBag.CurrentOrderBy = orderBy;
            ViewBag.SortParam = orderBy;

            if (search != null)
            {
                page = 1;
            }
            else
            {
                search = currentFilter;
            }

            ViewBag.CurrentFilter = search;

            var allObj = await _unitOfWork.Size.GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                allObj = await _unitOfWork.Size.GetAll(filter: u => u.Name.Contains(search));
            }
            switch (orderBy)
            {
                case "name_asc":
                    allObj = allObj.OrderBy(u => u.Name);
                    break;
                case "name_desc":
                    allObj = allObj.OrderByDescending(u => u.Name);
                    break;
            }
            int pageSize = 10;
            return View(PaginatedList<Size>.Create(allObj, page ?? 1, pageSize));
            //return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "_ViewAll", (PaginatedList<Category>.Create(allObj, page ?? 1, pageSize)) });

        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var objDetails = await _unitOfWork.Size.Get(id);
            if (objDetails == null)
            {
                return NotFound();
            }
            return View(objDetails);
        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var objToDelete = await _unitOfWork.Size.Get(id);
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
            var objToDelete = await _unitOfWork.Size.Get(id);
            await _unitOfWork.Size.Remove(objToDelete);
            await _unitOfWork.Save();
            return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.Size.GetAll()) });
        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Upsert(int? id)
        {
            Size size = new Size();
            if (id == null)
            {
                //new category
                return View(size);
            }
            size = await _unitOfWork.Size.Get(id.GetValueOrDefault());
            if (size == null)
            {
                return NotFound();
            }
            //edit category
            return View(size);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert([Bind("Id, Name")] Size size)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (size.Id == 0)
                    {
                        await _unitOfWork.Size.Add(size);
                    }
                    else
                    {
                        _unitOfWork.Size.Update(size);
                    }
                }
                catch (DBConcurrencyException)
                {
                    if (!SizeExists(size.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                await _unitOfWork.Save();
                return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.Size.GetAll()) });
            }
            return Json(new { isValid = false, html = PopupHelper.RenderRazorViewToString(this, "Upsert", size) });
        }
        public bool SizeExists(int id)
        {
            return _context.Sizes.Any(u => u.Id == id);
        }
    }
}
