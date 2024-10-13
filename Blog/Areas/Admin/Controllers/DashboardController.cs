using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Models.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
        public DashboardController(IUniteOfWork uniteOfWork)
        {
            _uniteOfWork = uniteOfWork;
        }
        public async Task<IActionResult> Index()
        {

            DashboardVM dashboardVM = new DashboardVM();

            dashboardVM.Users = await _uniteOfWork.ApplicationUser.GetAllAsync();
            dashboardVM.Order = await _uniteOfWork.Order.GetAllAsync(includeProperties: "OrderItems.Product");

            return View(dashboardVM);
        }
    }
}
