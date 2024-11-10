using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.Models;
using Blog.Models.VM;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Diagnostics;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUniteOfWork _uniteOfWork;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, IUniteOfWork uniteOfWork, ApplicationDbContext db)
        {
            _logger = logger;
            _uniteOfWork = uniteOfWork;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _uniteOfWork.Product.GetAllAsync(p => p.IsActive == true, includeProperties: "Category,Faculty");
            var blogs = await _uniteOfWork.Blog.GetAllAsync();
            var sosalMedia = await _uniteOfWork.SosalMedia.GetAllAsync();
            var testmonials = await _db.Testmonials.ToListAsync();


            var homePageVM = new HomePageVM
            {
                BlogCategoryViewModel = blogs.GroupBy(b => b.CategoryName)
                            .Select(group => new BlogCategoryViewModel
                            {
                                CategoryName = group.Key,
                                Blogs = group.ToList()
                            })
                            .ToList(),
                Products = products.OrderBy(b => b.CreatedOn).Take(10),
                Testmonials = testmonials
            };

            if(homePageVM.SosalMedia == null)
            {
                homePageVM.SosalMedia = new SosalMedia();
            }


            return View(homePageVM);
        }  

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(Contact contact)
        {
            if(!ModelState.IsValid)
            {
                return View(contact);
            }

            contact.CratedOn = DateTime.Now;

            await _uniteOfWork.Contact.AddAsync(contact);
            _uniteOfWork.Save();
            TempData["success"] = "Successfully send your message";
            return RedirectToAction("Contact", "Home");
        }

        public async Task<IActionResult> Blogs()
        {
            var blogs = await _uniteOfWork.Blog.GetAllAsync();
            return View(blogs.ToList());
        }

        public async Task<IActionResult> BlogDetails(int id)
        {
            if(id <= 0)
            {
                NotFound();
            }
            var blog = await _uniteOfWork.Blog.GetAsync(b => b.Id == id);

            if(blog == null)
            {
                NotFound();
            }
            
            return View(blog);
        }


        public IActionResult Achivements()
        {
            List<Achevements> list = _db.Achevements
                                 .FromSqlInterpolated($"SELECT * FROM Achevements ORDER BY CAST(SortedOrder AS INT) DESC")
                                 .ToList();

            return View(list);

        }


        public IActionResult Upcoming()
        {
            List<Upcoming> list = _db.Upcomings
                                 .FromSqlInterpolated($"SELECT * FROM Upcomings ORDER BY CAST(SortedOrder AS INT) DESC")
                                 .ToList();
            return View(list.OrderByDescending(u => u.SortedOrder).ToList());
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult RefundPolicy()
        {
            return View();
        }

        public IActionResult TermsAndCondition()
        {
            return View();
        }

        public async Task<IActionResult> YourMentor()
        {
            IEnumerable<Faculty> allMentor = await _uniteOfWork.Faculty.GetAllAsync(f => f.IsShowMentorPage == true);
            allMentor = allMentor.OrderByDescending(x => x.SortedOrder).ToList();  
            return View(allMentor);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
