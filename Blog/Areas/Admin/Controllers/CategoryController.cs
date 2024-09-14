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

        public IActionResult Index()
        {
            IEnumerable<Category> categoryList = _unitOfWork.Category.GetAll();
            return View(categoryList);
        }

        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if (id == null)
            {
                return Json(category);
            }
            else
            {
                category = _unitOfWork.Category.Get(c => c.Id == id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found!" });
                }
                return Json(category);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                var isDuplicate = _unitOfWork.Category.Get(c =>
                    (c.CategoryName == category.CategoryName || c.SortedOrder == category.SortedOrder)
                    && c.Id != category.Id);

                if (isDuplicate != null)
                {
                    return Json(new { success = false, message = "A category with the same name or sorted order already exists!" });
                }

                if (category.Id == 0)
                {
                    category.CreatedOn = DateTime.Now;
                    _unitOfWork.Category.Add(category);
                    _unitOfWork.Save();
                    return Json(new { success = true, message = "Category added successfully!", category = category });
                }
                else
                {
                    var existingCategory = _unitOfWork.Category.Get(c => c.Id == category.Id);
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
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _unitOfWork.Category.Get(u => u.Id == id);
            if (category != null)
            {
                _unitOfWork.Category.Delete(category);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Category deleted successfully!" });
            }
            return Json(new { success = false, message = "Category not found!" });
        }
    }
}