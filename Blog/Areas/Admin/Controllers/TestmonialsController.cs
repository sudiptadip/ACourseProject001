using Blog.DataAccess.Data;
using Blog.Models.Models;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TestmonialsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TestmonialsController(ApplicationDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {
            var list = _db.Testmonials.ToList();
            return View(list);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Testmonials model)
        {
            if (ModelState.IsValid)
            {
                _db.Testmonials.Add(model);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }


        public IActionResult Edit(int id)
        {
            var testimonial = _db.Testmonials.FirstOrDefault(t => t.Id == id);
            if (testimonial == null)
            {
                return NotFound();
            }
            return View(testimonial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Testmonials model)
        {
            if (ModelState.IsValid)
            {
                _db.Testmonials.Update(model);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        } 


        public IActionResult Delete(int id)
        {
            var testimonial = _db.Testmonials.FirstOrDefault(t => t.Id == id);
            if (testimonial != null)
            {
                _db.Testmonials.Remove(testimonial);
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}