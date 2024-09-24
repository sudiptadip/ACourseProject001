using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Models.VM;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using System.Diagnostics;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUniteOfWork _uniteOfWork;

        public HomeController(ILogger<HomeController> logger, IUniteOfWork uniteOfWork)
        {
            _logger = logger;
            _uniteOfWork = uniteOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _uniteOfWork.Product.GetAllAsync(includeProperties: "Category,Faculty");
            var blogs = await _uniteOfWork.Blog.GetAllAsync();
           // var sosalMedia = await _uniteOfWork.SosalMedia.GetAllAsync();

            var homePageVM = new HomePageVM
            {
                BlogCategoryViewModel = blogs.GroupBy(b => b.CategoryName)
                            .Select(group => new BlogCategoryViewModel
                            {
                                CategoryName = group.Key,
                                Blogs = group.ToList()
                            })
                            .ToList(),
                Products = products.OrderBy(b => b.CreatedOn).Take(10)
            };

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
            return View();
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
            var allMentor = await _uniteOfWork.Faculty.GetAllAsync();
            return View(allMentor);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
