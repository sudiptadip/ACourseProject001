﻿using Microsoft.AspNetCore.Mvc;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details()
        {
            return View();
        }
    }
}
