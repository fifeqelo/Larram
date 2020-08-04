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
using Microsoft.EntityFrameworkCore;

namespace Larram.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext context)
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

            var userList = await _context.ApplicationUsers.ToListAsync();
            var userRole = await _context.UserRoles.ToListAsync();
            var roles = await _context.Roles.ToListAsync();
            foreach(var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            }


            int pageSize = 10;
            return View(PaginatedList<ApplicationUser>.Create(userList, page ?? 1, pageSize));
        }

        [PopupHelper.NoDirectAccess]
        public IActionResult LockUnlock(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var objFromDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objFromDb == null) return NotFound();
            return View();
        }

        [HttpPost, ActionName("LockUnlock")]
        [ValidateAntiForgeryToken]
        public IActionResult LockUnlockConfirmed(string id)
        {
            var objFromDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _context.SaveChanges();
            return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _context.ApplicationUsers.ToListAsync())});
        }
    }
}
