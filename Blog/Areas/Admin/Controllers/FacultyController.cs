using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Utility.Service;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FacultyController : Controller
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public FacultyController(IUniteOfWork unitOfWork, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Faculty> facultyList = await _unitOfWork.Faculty.GetAllAsync();
            return View(facultyList);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Faculty faculty = new Faculty();
            if (id == null)
            {
                return View(faculty);
            }
            else
            {
                faculty = await _unitOfWork.Faculty.GetAsync(c => c.Id == id);
                if (faculty == null)
                {
                    return View(faculty);
                }
                return View(faculty);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Faculty faculty, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicates
                var isDuplicate = await _unitOfWork.Faculty.GetAsync(c =>
                    (c.FacultyName == faculty.FacultyName || c.SortedOrder == faculty.SortedOrder)
                    && c.Id != faculty.Id);

                if (isDuplicate != null)
                {
                    ModelState.AddModelError("", "Faculty name or sorted order already exists.");
                    return View(faculty);
                }

                // Handle Image Upload
                if (file != null && file.Length > 0)
                {
                    // Optionally, delete the old image if updating
                    if (faculty.Id != 0)
                    {
                        var existingFaculty = await _unitOfWork.Faculty.GetAsync(c => c.Id == faculty.Id);
                        if (existingFaculty != null && !string.IsNullOrEmpty(existingFaculty.ImageUrl))
                        {
                            await _imageService.DeleteImageAsync(Path.GetFileName(existingFaculty.ImageUrl));
                        }
                    }

                    var imageUrl = await _imageService.UploadSingleImageAsync(file);
                    if (imageUrl != null)
                    {
                        faculty.ImageUrl = imageUrl;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Image upload failed.");
                        return View(faculty);
                    }
                }

                if (faculty.Id == 0)
                {
                    faculty.CreatedOn = DateTime.Now;
                   await _unitOfWork.Faculty.AddAsync(faculty);
                }
                else
                {
                    var existingFaculty = await _unitOfWork.Faculty.GetAsync(c => c.Id == faculty.Id);
                    if (existingFaculty == null)
                    {
                        return NotFound();
                    }

                    existingFaculty.FacultyName = faculty.FacultyName;
                    existingFaculty.Description = faculty.Description;
                    existingFaculty.SortedOrder = faculty.SortedOrder;
                    existingFaculty.ImageUrl = faculty.ImageUrl;
                    existingFaculty.ModifiedOn = DateTime.Now;
                    _unitOfWork.Faculty.Update(existingFaculty);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(faculty);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var product = await _unitOfWork.Product.GetAsync(c => c.FacultyId == id);

            if (product != null)
            {
                TempData["error"] = "Faculty Assosiated With Product";
                return RedirectToAction(nameof(Index));
            }

            var faculty = await _unitOfWork.Faculty.GetAsync(u => u.Id == id);
            if (faculty != null)
            {
                // Delete the associated image if it exists
                if (!string.IsNullOrEmpty(faculty.ImageUrl))
                {
                    string fileName = Path.GetFileName(faculty.ImageUrl);
                    bool isDeleted = await _imageService.DeleteImageAsync(fileName);
                    if (!isDeleted)
                    {
                        // Optionally, log the failure to delete the image
                        return Json(new { success = false, message = "Failed to delete the associated image." });
                    }
                }

              await  _unitOfWork.Faculty.DeleteAsync(faculty);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Faculty deleted successfully!" });
            }
            return Json(new { success = false, message = "Faculty not found!" });
        }
    }
}