using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SubjectController : Controller
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;

        public SubjectController(IUniteOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Subject> subjectList = await _unitOfWork.Subject.GetAllAsync();

            List<Subject> subjectListWithCategoryName = new List<Subject>();


            foreach (var item in subjectList)
            {
                var category = _db.Categories.FirstOrDefault(u => u.Id == item.CategoryId);
                var categoryName = category != null ? category.CategoryName : "";
                item.CategoryName = categoryName;
                subjectListWithCategoryName.Add(item);
            }



            var categoryList = await _unitOfWork.Category.GetAllAsync();
            List<SelectListItem> categorySelect = categoryList.Select(c => new SelectListItem
            {
                Text = c.CategoryName,
                Value = c.Id.ToString()
            }).ToList();
            ViewBag.categorySelectList = categorySelect;

            return View(subjectListWithCategoryName);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Subject subject = new Subject();
            if (id == null)
            {
                return Json(subject);
            }
            else
            {
                subject = await _unitOfWork.Subject.GetAsync(c => c.Id == id);
                if (subject == null)
                {
                    return Json(new { success = false, message = "Subject not found!" });
                }
                return Json(subject);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(Subject subject)
        {
            if (ModelState.IsValid)
            {
                var isDuplicate = await _unitOfWork.Subject.GetAsync(c =>
                    (c.SubjectName == subject.SubjectName || c.SortedOrder == subject.SortedOrder)
                    && c.Id != subject.Id);

                if (isDuplicate != null)
                {
                    return Json(new { success = false, message = "A Subject with the same name or sorted order already exists!" });
                }

                if (subject.Id == 0)
                {
                    subject.CreatedOn = DateTime.Now;
                    _unitOfWork.Subject.AddAsync(subject);
                    _unitOfWork.Save();
                    return Json(new { success = true, message = "Subject added successfully!", subject = subject });
                }
                else
                {
                    var existingSubject = await _unitOfWork.Subject.GetAsync(c => c.Id == subject.Id);
                    if (existingSubject == null)
                    {
                        return Json(new { success = false, message = "Subject not found!" });
                    }

                    // Update existing category
                    existingSubject.SubjectName = subject.SubjectName;
                    existingSubject.SortedOrder = subject.SortedOrder;
                    existingSubject.CategoryId = subject.CategoryId;

                    _db.Subjects.Update(existingSubject);
                    _db.SaveChanges();

                    return Json(new { success = true, message = "Subject updated successfully!", subject = existingSubject });
                }
            }
            return Json(new { success = false, message = "Invalid data provided!" });
        }


        [HttpPost]
       public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _unitOfWork.Product.GetAsync(c => c.SubjectId == id);

            if (product != null)
            {
                TempData["error"] = "Subject Assosiated With Product";
                return RedirectToAction(nameof(Index));
            }

            var subject = await _unitOfWork.Subject.GetAsync(u => u.Id == id);
            if (subject != null)
            {
                await  _unitOfWork.Subject.DeleteAsync(subject);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Subject deleted successfully!" });
            }
            return Json(new { success = false, message = "Subject not found!" });
        }
    }
}