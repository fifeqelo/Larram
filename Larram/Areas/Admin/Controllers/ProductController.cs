using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Larram.DataAccess.Data;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
using Larram.Models.ViewModels;
using Larram.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Larram.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnviroment;
        [BindProperty]
        public ProductViewModel productViewModel { get; set; }

        public ProductController(IUnitOfWork unitOfWork, ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _hostEnviroment = hostEnvironment;
        }

        public async Task<IActionResult> Index(string orderBy, string search, string currentFilter, int? page)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(orderBy) ? "name_desc" : "";
            ViewData["CategorySortParm"] = orderBy == "Category" ? "category_desc" : "Category";
            ViewData["PriceSortParm"] = orderBy == "Price" ? "price_desc" : "Price";
            ViewData["DiscountPriceSortParm"] = orderBy == "DiscountPrice" ? "discountPrice_desc" : "DiscountPrice";
            ViewData["DateSortParm"] = orderBy == "Date" ? "date_desc" : "Date";
            ViewData["CurrentSort"] = orderBy;

            if (search != null)
            {
                page = 1;
            }
            else
            {
                search = currentFilter;
            }

            ViewData["CurrentFilter"] = search;

            var allObj = await _unitOfWork.Product.GetAll(includeProperties:"Category,Color");
            if (!String.IsNullOrEmpty(search))
            {
                allObj = await _unitOfWork.Product.GetAll(filter: u => u.Name.Contains(search));
            }
            switch (orderBy)
            {
                case "name_desc":
                    allObj = allObj.OrderByDescending(u => u.Name);
                    break;
                case "category_desc":
                    allObj = allObj.OrderByDescending(u => u.Category.Name);
                    break;
                case "Category":
                    allObj = allObj.OrderBy(u => u.Category.Name);
                    break;
                case "price_desc":
                    allObj = allObj.OrderByDescending(u => u.Price);
                    break;
                case "Price":
                    allObj = allObj.OrderBy(u => u.Price);
                    break;
                case "discountPrice_desc":
                    allObj = allObj.OrderByDescending(u => u.DiscountPrice);
                    break;
                case "DiscountPrice":
                    allObj = allObj.OrderBy(u => u.DiscountPrice);
                    break;
                case "date_desc":
                    allObj = allObj.OrderByDescending(u => u.CreatedDate);
                    break;
                case "Date":
                    allObj = allObj.OrderBy(u => u.CreatedDate);
                    break;
                default:
                    allObj = allObj.OrderBy(u => u.Name);
                    break;
            }
            int pageSize = 10;
            return View(PaginatedList<Product>.Create(allObj, page ?? 1, pageSize));
        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var objDetails = await _unitOfWork.Product.GetFirstOrDefault(d => d.Id == id, includeProperties:"Category,Color");
            if(objDetails == null)
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
            var objToDelete = await _unitOfWork.Product.Get(id);
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
            var objToDelete = await _unitOfWork.Product.Get(id);
            var availabilityToDelete = await _unitOfWork.ProductAvailability.GetAll(filter: u => u.ProductId == id);
            await _unitOfWork.Product.Remove(objToDelete);
            await _unitOfWork.ProductAvailability.RemoveRange(availabilityToDelete);
            await _unitOfWork.Save();
            return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.Product.GetAll()) });
        }

        [PopupHelper.NoDirectAccess]
        public async Task<IActionResult> Upsert(int? id)
        {
            ProductViewModel productViewModel = new ProductViewModel()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAllNotAsync().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                GenderList = Enum.GetValues(typeof(Gender)).Cast<Gender>().Select(e => new SelectListItem 
                { 
                    Value = e.ToString(), 
                    Text = e.ToString() 
                }),
                ColorList = _unitOfWork.Color.GetAllNotAsync().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };
            if(id == null)
            {
                //new product
                return View(productViewModel);
            }
            productViewModel.Product = await _unitOfWork.Product.Get(id.GetValueOrDefault());
            if(productViewModel == null)
            { 
                return NotFound();
            }
            //edit product
            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert()
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnviroment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                { 
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    var extenstion = Path.GetExtension(files[0].FileName);

                    if (productViewModel.Product.ImageUrl != null)
                    {
                        //remove old image
                        var imagePath = Path.Combine(webRootPath, productViewModel.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }
                    productViewModel.Product.ImageUrl = @"\images\products\" + fileName + extenstion;
                }
                else
                {
                    //update when no image change
                    if (productViewModel.Product.Id != 0)
                    {
                        Product objFromDb = await _unitOfWork.Product.Get(productViewModel.Product.Id);
                        productViewModel.Product.ImageUrl = objFromDb.ImageUrl;
                    }
                }
                try
                {
                    if(productViewModel.Product.Id == 0)
                    {
                        if(productViewModel.Product.DiscountPrice != 0 && productViewModel.Product.DiscountPrice < productViewModel.Product.Price)
                        {
                            await _unitOfWork.Product.Add(productViewModel.Product);
                        } else
                        {
                            productViewModel.Product.DiscountPrice = productViewModel.Product.Price;
                            await _unitOfWork.Product.Add(productViewModel.Product);
                        }
                    }
                    else
                    {
                        if (productViewModel.Product.DiscountPrice != 0 && productViewModel.Product.DiscountPrice < productViewModel.Product.Price)
                        {
                            _unitOfWork.Product.Update(productViewModel.Product);
                        }else
                        {
                            productViewModel.Product.DiscountPrice = productViewModel.Product.Price;
                            _unitOfWork.Product.Update(productViewModel.Product);
                        }
                    }
                }
                catch (DBConcurrencyException)
                {
                    if (!ProductExists(productViewModel.Product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                await _unitOfWork.Save();
                return Json(new { isValid = true, html = PopupHelper.RenderRazorViewToString(this, "Index", _unitOfWork.Product.GetAll()) });
            }
            return Json(new { isValid = false, html = PopupHelper.RenderRazorViewToString(this, "Upsert", productViewModel.Product) });
        }
        public bool ProductExists(int id)
        {
            return _context.Products.Any(u => u.Id == id);
        }
    }
}
