using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
using Microsoft.AspNetCore.Mvc;

namespace Larram.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            int pageSize = 5;
            return View(PaginatedList<Category>.Create(allObj, page ?? 1, pageSize));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var objToDelete = await _unitOfWork.Category.GetFirstOrDefault(o => o.Id == id);
            if(objToDelete == null)
            {
                return NotFound();
            }
            return View(objToDelete);
        }
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var objToDelete = await _unitOfWork.Category.GetFirstOrDefault(o => o.Id == id);
            await _unitOfWork.Category.Remove(objToDelete);
            await _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
