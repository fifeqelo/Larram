using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Larram.DataAccess.Data;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
using Larram.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Larram.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public CategoryController(IUnitOfWork unitOfWork, ApplicationDbContext context)
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

            var allObj = await _unitOfWork.Category.GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                allObj = await _unitOfWork.Category.GetAll(filter: u => u.Name.Contains(search));
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
            return View(PaginatedList<Category>.Create(allObj, page ?? 1, pageSize));
            //return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "_ViewAll", (PaginatedList<Category>.Create(allObj, page ?? 1, pageSize)) });

        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var objToDelete = await _unitOfWork.Category.Get(id);
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
            var objToDelete = await _unitOfWork.Category.Get(id);
            await _unitOfWork.Category.Remove(objToDelete);
            await _unitOfWork.Save();
            return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.Category.GetAll()) });
        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Upsert(int? id)
        {
            Category category = new Category();
            if(id == null)
            {
                //new category
                return View(category);
            }
           category = await _unitOfWork.Category.Get(id.GetValueOrDefault());
            if(category == null)
            { 
                return NotFound();
            }
            //edit category
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert([Bind("Id, Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(category.Id == 0)
                    {
                        await _unitOfWork.Category.Add(category);
                    }
                    else
                    {
                        _unitOfWork.Category.Update(category);
                    }
                }
                catch (DBConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                await _unitOfWork.Save();
                return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.Category.GetAll()) });
            }
            return Json(new { isValid = false, html = PopupHelper.RenderRazorViewToString(this, "Upsert", category) });
        }
        public bool CategoryExists(int id)
        {
            return _context.Categories.Any(u => u.Id == id);
        }
    }
}
