using Blog.DataAccess.Data;
using Blog.Models.Models;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AchevementsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService _imageService;

        public AchevementsController(ApplicationDbContext db, IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }

        // List view
        public IActionResult Index()
        {
            List<Achevements> list = _db.Achevements.ToList();
            list = list.OrderBy(u => u.SortedOrder).ToList();
            return View(list);
        }

        // Upsert (Create or Update) Achievement
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (id == null)
            {
                return View(new Achevements()); // return empty model for new creation
            }

            var achievement = await _db.Achevements.FindAsync(id);
            if (achievement == null)
            {
                TempData["error"] = "Record not found";
                return RedirectToAction("Index");
            }

            return View(achievement); // return existing model for editing
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(IFormFile file, Achevements modal)
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
                    _db.Achevements.Add(modal);
                    TempData["success"] = "Successfully added";
                }
                else
                {
                    var existingAchievement = await _db.Achevements.FindAsync(modal.Id);
                    if (existingAchievement != null)
                    {
                        existingAchievement.SortedOrder = modal.SortedOrder;

                        if (!string.IsNullOrEmpty(modal.ImageUrl))
                        {
                            existingAchievement.ImageUrl = modal.ImageUrl;
                        }

                        _db.Achevements.Update(existingAchievement);
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
            var achievement = await _db.Achevements.FindAsync(id);
            if (achievement == null)
            {
                TempData["error"] = "Record not found";
                return RedirectToAction("Index");
            }

            _db.Achevements.Remove(achievement);
            await _db.SaveChangesAsync();

            TempData["success"] = "Successfully deleted";
            return RedirectToAction("Index");
        }
    }
}