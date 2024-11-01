using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Utility.Service;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public BlogController(IUniteOfWork unitOfWork, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Blogs> blogList = await _unitOfWork.Blog.GetAllAsync();
            return View(blogList);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Blogs blog = new Blogs();
            if (id == null)
            {
                return View(blog);
            }
            else
            {
                blog = await _unitOfWork.Blog.GetAsync(c => c.Id == id);
                if (blog == null)
                {
                    return View(blog);
                }
                return View(blog);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Blogs blogs, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicates
                var isDuplicate = await _unitOfWork.Blog.GetAsync(c =>
                    (c.Title == blogs.Title || c.SortedOrder == blogs.SortedOrder)
                    && c.Id != blogs.Id);

                if (isDuplicate != null)
                {
                    TempData["error"] = "Faculty name or sorted order already exists.";
                    ModelState.AddModelError("", "Faculty name or sorted order already exists.");
                    return View(blogs);
                }

                if (file != null && file.Length > 0)
                {
                    if (blogs.Id != 0)
                    {
                        var existingblog = await _unitOfWork.Blog.GetAsync(c => c.Id == blogs.Id);
                        if (existingblog != null && !string.IsNullOrEmpty(existingblog.ImageUrl))
                        {
                            await _imageService.DeleteImageAsync(Path.GetFileName(existingblog.ImageUrl));
                        }
                    }

                    var imageUrl = await _imageService.UploadSingleImageAsync(file);
                    if (imageUrl != null)
                    {
                        blogs.ImageUrl = imageUrl;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Image upload failed.");
                        TempData["error"] = "Image upload failed.";
                        return View(blogs);
                    }
                }

                if (blogs.Id == 0)
                {
                    blogs.CreatedOn = DateTime.Now;
                   await _unitOfWork.Blog.AddAsync(blogs);
                   TempData["success"] = "Successfully Blog Created";
                }
                else
                {
                    var existingBlog = await _unitOfWork.Blog.GetAsync(c => c.Id == blogs.Id);
                    if (existingBlog == null)
                    {
                        return NotFound();
                    }

                    existingBlog.Title = blogs.Title;
                    existingBlog.Description = blogs.Description;
                    existingBlog.SortedOrder = blogs.SortedOrder;
                    existingBlog.CategoryName = blogs.CategoryName;

                    if (!String.IsNullOrEmpty(blogs.ImageUrl))
                    {
                        existingBlog.ImageUrl = blogs.ImageUrl;
                    }

                    _unitOfWork.Blog.Update(existingBlog);
                    TempData["success"] = "Successfully Blog update";
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(blogs);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _unitOfWork.Blog.GetAsync(u => u.Id == id);
            if (blog != null)
            {
                if (!string.IsNullOrEmpty(blog.ImageUrl))
                {
                    string fileName = Path.GetFileName(blog.ImageUrl);
                    bool isDeleted = await _imageService.DeleteImageAsync(fileName);
                    if (!isDeleted)
                    {
                        return Json(new { success = false, message = "Failed to delete the associated image." });
                    }
                }

              await  _unitOfWork.Blog.DeleteAsync(blog);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Faculty deleted successfully!" });
            }
            return Json(new { success = false, message = "Faculty not found!" });
        }
    }
}