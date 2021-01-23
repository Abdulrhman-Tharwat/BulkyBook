using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int productPage = 1)
        {
            CategoryVM categoryVM = new CategoryVM()
            {
                Categories = await _unitOfWork.Category.GetAllAsync()
            };

            var count = categoryVM.Categories.Count();
            categoryVM.Categories = categoryVM.Categories.OrderBy(p => p.Name).Skip((productPage - 1) * 2).Take(2).ToList();
            categoryVM.PagingInfo = new PagingInfo()
            {
                Currentpage = productPage,
                ItemPerPage = 2,
                TotalItem = count,
                urlParam = "/Admin/Category/Index?productPage=:"
            };

            return View(categoryVM);
        }
        public async Task<IActionResult> Upsert(int? id)
        {
            Category category = new Category();
            if (id == null)
            {
                return View(category);
            }
            category = await _unitOfWork.Category.GetAsync(id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    await _unitOfWork.Category.AddAsync(category);
                }
                else
                {
                    _unitOfWork.Category.Update(category);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        #region API Call
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var obj = await _unitOfWork.Category.GetAllAsync();
            return Json(new { data = obj });
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var Obj = await _unitOfWork.Category.GetAsync(id);
            if (Obj == null)
            {
                TempData["Error"] = "Error Deleting Category";
                return Json(new { success = false, message = "Error" });
            }
            await _unitOfWork.Category.RemoveAsync(Obj);
            _unitOfWork.Save();
            TempData["Success"] = "Category successfuly Deleted";
            return Json(new { success = true, message = "Delete successful" });

        }


        #endregion
    }
}
