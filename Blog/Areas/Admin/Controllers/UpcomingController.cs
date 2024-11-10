using Blog.DataAccess.Data;
using Blog.Models.Models;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UpcomingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService _imageService;

        public UpcomingController(ApplicationDbContext db, IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }

        // List view
        public IActionResult Index()
        {
            List<Upcoming> list = _db.Upcomings.ToList();
            list = list.OrderBy(u => u.SortedOrder).ToList();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (id == null)
            {
                return View(new Upcoming()); 
            }

            var upcoming = await _db.Upcomings.FindAsync(id);
            if (upcoming == null)
            {
                TempData["error"] = "Record not found";
                return RedirectToAction("Index");
            }

            return View(upcoming); // return existing model for editing
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(IFormFile file, Upcoming modal)
        {
            try
            {
                if (file != null)
                {
                    string imagePath = await _imageService.UploadSingleImageAsync(file);
                    modal.ImageUrl = imagePath;
                }

                if (modal.Id == 0)
                {
                    _db.Upcomings.Add(modal);
                    TempData["success"] = "Successfully added";
                }
                else
                {
                    var existingUpcoming = await _db.Upcomings.FindAsync(modal.Id);
                    if (existingUpcoming != null)
                    {
                        existingUpcoming.SortedOrder = modal.SortedOrder;

                        if (!string.IsNullOrEmpty(modal.ImageUrl))
                        {
                            existingUpcoming.ImageUrl = modal.ImageUrl;
                        }

                        _db.Upcomings.Update(existingUpcoming);
                        TempData["success"] = "Successfully updated";
                    }
                    else
                    {
                        TempData["error"] = "Record not found";
                        return RedirectToAction("Index");
                    }
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message.ToString();
                return View(modal);
            }
        }


        public async Task<IActionResult> Delete(int id)
        {
            var upcoming = await _db.Upcomings.FindAsync(id);
            if (upcoming == null)
            {
                TempData["error"] = "Record not found";
                return RedirectToAction("Index");
            }

            _db.Upcomings.Remove(upcoming);
            await _db.SaveChangesAsync();

            TempData["success"] = "Successfully deleted";
            return RedirectToAction("Index");
        }
    }
}