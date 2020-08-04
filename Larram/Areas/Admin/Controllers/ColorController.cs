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
    public class ColorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public ColorController(IUnitOfWork unitOfWork, ApplicationDbContext context)
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

            var allObj = await _unitOfWork.Color.GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                allObj = await _unitOfWork.Color.GetAll(filter: u => u.Name.Contains(search));
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
            return View(PaginatedList<Color>.Create(allObj, page ?? 1, pageSize));
            //return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "_ViewAll", (PaginatedList<Color>.Create(allObj, page ?? 1, pageSize)) });

        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var objToDelete = await _unitOfWork.Color.Get(id);
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
            var objToDelete = await _unitOfWork.Color.Get(id);
            await _unitOfWork.Color.Remove(objToDelete);
            await _unitOfWork.Save();
            return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.Color.GetAll()) });
        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Upsert(int? id)
        {
            Color color = new Color();
            if(id == null)
            {
                //new color
                return View(color);
            }
           color = await _unitOfWork.Color.Get(id.GetValueOrDefault());
            if(color == null)
            { 
                return NotFound();
            }
            //edit color
            return View(color);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert([Bind("Id, Name, HexValue")] Color color)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(color.Id == 0)
                    {
                        await _unitOfWork.Color.Add(color);
                    }
                    else
                    {
                        _unitOfWork.Color.Update(color);
                    }
                }
                catch (DBConcurrencyException)
                {
                    if (!ColorExists(color.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                await _unitOfWork.Save();
                return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.Color.GetAll()) });
            }
            return Json(new { isValid = false, html = PopupHelper.RenderRazorViewToString(this, "Upsert", color) });
        }
        public bool ColorExists(int id)
        {
            return _context.Colors.Any(u => u.Id == id);
        }
    }
}
