using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.Models;
using Blog.Models.VM;
using Blog.Utility.Service;
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
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, IUniteOfWork uniteOfWork, ApplicationDbContext db, IEmailService emailService)
        {
            _logger = logger;
            _uniteOfWork = uniteOfWork;
            _db = db;
            _emailService = emailService;
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


            string contactEmailTemplate = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n  <title>Contact Us Form Submission</title>\r\n</head>\r\n<body style=\"font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f9f9f9; color: #333;\">\r\n  <div style=\"width: 100%; max-width: 600px; margin: 20px auto; background: #ffffff; border: 1px solid #ddd; border-radius: 8px; padding: 20px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);\">\r\n    <div style=\"text-align: center; padding-bottom: 20px; border-bottom: 1px solid #ddd;\">\r\n      <h1 style=\"font-size: 24px; margin: 0; color: #0077b6;\">Get In Touch</h1>\r\n    </div>\r\n    <div style=\"padding: 20px;\">\r\n      <p style=\"font-size: 16px; margin-bottom: 10px;\">Hello,</p>\r\n      <p style=\"font-size: 14px; margin-bottom: 10px;\">You have received a new message through the contact form:</p>\r\n      <table style=\"width: 100%; border-collapse: collapse; margin-top: 10px;\">\r\n        <tr>\r\n          <td style=\"font-size: 14px; font-weight: bold; padding: 5px 0; border-bottom: 1px solid #ddd;\">Name:</td>\r\n          <td style=\"font-size: 14px; padding: 5px 0; border-bottom: 1px solid #ddd;\">{contact.Name}</td>\r\n        </tr>\r\n        <tr>\r\n          <td style=\"font-size: 14px; font-weight: bold; padding: 5px 0; border-bottom: 1px solid #ddd;\">Email:</td>\r\n          <td style=\"font-size: 14px; padding: 5px 0; border-bottom: 1px solid #ddd;\">{contact.Email}</td>\r\n        </tr>\r\n        <tr>\r\n          <td style=\"font-size: 14px; font-weight: bold; padding: 5px 0; border-bottom: 1px solid #ddd;\">Phone:</td>\r\n          <td style=\"font-size: 14px; padding: 5px 0; border-bottom: 1px solid #ddd;\">{contact.PhoneNumber}</td>\r\n        </tr>\r\n        <tr>\r\n          <td style=\"font-size: 14px; font-weight: bold; padding: 5px 0; border-bottom: 1px solid #ddd;\">Message:</td>\r\n          <td style=\"font-size: 14px; padding: 5px 0; border-bottom: 1px solid #ddd;\">{contact.Message}</td>\r\n        </tr>\r\n      </table>\r\n      <p style=\"font-size: 14px; margin-top: 20px;\">Best regards,</p>\r\n      <p style=\"font-size: 14px;\"></p>\r\n    </div>\r\n    <div style=\"text-align: center; padding-top: 20px; border-top: 1px solid #ddd;\">\r\n      <p style=\"font-size: 12px; color: #999;\">This email was generated automatically. Please do not reply to this email.</p>\r\n    </div>\r\n  </div>\r\n</body>\r\n</html>\r\n";

            _emailService.SendEmail(new EmailDto
            {
                Subject = "NEW MESSAGE",
                Body = contactEmailTemplate,
                To = "topclasses10@gmail.com"
            });

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
            allMentor = allMentor.OrderBy(x => x.SortedOrder).ToList();  
            return View(allMentor);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
