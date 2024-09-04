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

        // GET: Customer/Category
        public IActionResult Index()
        {
            IEnumerable<Category> categoryList = _unitOfWork.Category.GetAll();
            return View(categoryList);
        }

        // GET: Customer/Category/Upsert/5
        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if (id == null)
            {
                // This will be a create request
                return View(category);
            }
            // This will be an update request
            category = _unitOfWork.Category.Get(c => c.Id == id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Customer/Category/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    _unitOfWork.Category.Add(category);
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

        // GET: Customer/Category/Delete/5
        public IActionResult Delete(int? id)
        {
            var category = _unitOfWork.Category.Get(c => c.Id == id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Customer/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            var category = _unitOfWork.Category.Get(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Delete(category);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
