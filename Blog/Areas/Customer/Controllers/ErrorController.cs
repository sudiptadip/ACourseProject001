using Microsoft.AspNetCore.Mvc;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ErrorController : Controller
    {
        public IActionResult PageNotFound()
        {
            return View();
        }

        public IActionResult InternalServerError()
        {
            return View();
        }
    }
}
