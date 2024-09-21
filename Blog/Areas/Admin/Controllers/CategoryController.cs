using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUniteOfWork _unitOfWork;

        public CategoryController(IUniteOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Category> categoryList = await _unitOfWork.Category.GetAllAsync();
            return View(categoryList);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Category category = new Category();
            if (id == null)
            {
                return Json(category);
            }
            else
            {
                category = await _unitOfWork.Category.GetAsync(c => c.Id == id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found!" });
                }
                return Json(category);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                var isDuplicate = await _unitOfWork.Category.GetAsync(c =>
                    (c.CategoryName == category.CategoryName || c.SortedOrder == category.SortedOrder)
                    && c.Id != category.Id);

                if (isDuplicate != null)
                {
                    return Json(new { success = false, message = "A category with the same name or sorted order already exists!" });
                }

                if (category.Id == 0)
                {
                    category.CreatedOn = DateTime.Now;
                  await  _unitOfWork.Category.AddAsync(category);
                    _unitOfWork.Save();
                    return Json(new { success = true, message = "Category added successfully!", category = category });
                }
                else
                {
                    var existingCategory = await _unitOfWork.Category.GetAsync(c => c.Id == category.Id);
                    if (existingCategory == null)
                    {
                        return Json(new { success = false, message = "Category not found!" });
                    }

                    // Update existing category
                    existingCategory.CategoryName = category.CategoryName;
                    existingCategory.SortedOrder = category.SortedOrder;
                    existingCategory.IsActive = category.IsActive;
                    _unitOfWork.Category.Update(existingCategory);
                    _unitOfWork.Save();
                    return Json(new { success = true, message = "Category updated successfully!", category = existingCategory });
                }
            }
            return Json(new { success = false, message = "Invalid data provided!" });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _unitOfWork.Category.GetAsync(u => u.Id == id);
            if (category != null)
            {
               await _unitOfWork.Category.DeleteAsync(category);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Category deleted successfully!" });
            }
            return Json(new { success = false, message = "Category not found!" });
        }
    }
}