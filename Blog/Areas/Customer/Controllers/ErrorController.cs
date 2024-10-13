using Microsoft.AspNetCore.Mvc;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult PageNotFound()
        {
            return View("404");
        }

        [Route("Error/500")]
        public IActionResult InternalServerError()
        {
            return View("500");
        }
    }
}
