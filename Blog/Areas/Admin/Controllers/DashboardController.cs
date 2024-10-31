using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.Models;
using Blog.Models.VM;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace Blog.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        public DashboardController(IUniteOfWork uniteOfWork, ApplicationDbContext db, IEmailService emailService)
        {
            _uniteOfWork = uniteOfWork;
            _db = db;
            _emailService = emailService;
        }
        public async Task<IActionResult> Index()
        {

            DashboardVM dashboardVM = new DashboardVM();

            dashboardVM.Users = await _uniteOfWork.ApplicationUser.GetAllAsync();
            dashboardVM.Order = await _uniteOfWork.Order.GetAllAsync(includeProperties: "OrderItems.Product");
            dashboardVM.Order = dashboardVM.Order.OrderByDescending(u => u.CreatedAt);

            return View(dashboardVM);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, bool isPaymentDone, string email)
        {
            try
            {
                OrderItem orderItem = _db.OrderItems.Include(x => x.Product).FirstOrDefault(u => u.Id == id);

                orderItem.IsPaymentDone = isPaymentDone;

                _db.OrderItems.Update(orderItem);
                _db.SaveChanges();


                if(email != null)
                {
                    if(isPaymentDone)
                    {
                        _emailService.SendEmail(new EmailDto
                        {
                            To = email,
                            Subject = $"{orderItem.Product.ProductName} Course Active",
                            Body = $"{orderItem.Product.ProductName} Course Active"
                        });
                    }
                }               

                @TempData["success"] = "Successfully change order status";
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                @TempData["success"] = ex.Message.ToString();
                return RedirectToAction(nameof(Index));
            }

        }


    }
}
