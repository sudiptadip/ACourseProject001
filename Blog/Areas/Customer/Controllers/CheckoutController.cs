using Azure;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.Models;
using Blog.Models.VM;
using Blog.Utility.Service;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CheckoutController : Controller
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly EasebuzzService _easebuzzService;

        public CheckoutController(IUniteOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, EasebuzzService easebuzzService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _easebuzzService = easebuzzService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Fetch the cart for the user
            var cart = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, includeProperties: "CartItems.Product");
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var order = await _unitOfWork.Order.GetAsync(o => o.UserId == userId);

            if(order != null)
            {
                var checkoutViewModel = new CheckoutVM
                {
                    Cart = cart,
                    Address = order.Address,
                    City = order.City,
                    Country = order.Country,
                    Email = order.Email,
                    FirstName = order.FirstName,
                    LastName = order.LastName, 
                    PhoneNumber = order.PhoneNumber,
                    PostalCode = order.PostalCode,
                    State = order.State,
                };
                return View(checkoutViewModel);
            }
            else
            {
                var checkoutViewModel = new CheckoutVM
                {
                    Cart = cart,
                };

                return View(checkoutViewModel);

            }

        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutVM model)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var carts = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, includeProperties: "CartItems.Product");

            if (!ModelState.IsValid)
            {

                if (carts != null)
                {
                    model.Cart = carts;
                }
                else
                {
                    TempData["error"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }
                return View("Index", model);
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "txnid", Guid.NewGuid().ToString() },
                { "key", _easebuzzService.Key },
                { "amount", carts.CartItems.Sum(x => x.DiscountPrice).ToString() },
                { "firstname", model.FirstName },
                { "email", model.Email },
                { "phone", model.PhoneNumber },
                { "productinfo", "Topclasses Product" },
                { "surl", $"https://localhost:44391/Customer/Checkout/PaymentSuccessfull?firstName={model.FirstName}&lastName={model.LastName}&email={model.Email}&address={model.Address}&phoneNumber={model.PhoneNumber}&city={model.City}&state={model.State}&postalCode={model.PostalCode}&country={model.Country}" },
                { "furl", "https://localhost:44391/Customer/Checkout/Failde" }
            };

            parameters["hash"] = GenerateHash(parameters, _easebuzzService.Salt);

            string result = _easebuzzService.InitiatePayment(parameters);

            PaymentJsonResponse res = JsonConvert.DeserializeObject<PaymentJsonResponse>(result);

            return Redirect($"https://pay.easebuzz.in/pay/" + res.data);

        }

        public async Task<IActionResult> Failde()
        {
            return View();
        }


        public async Task<IActionResult> Success()
        {
            return View();
        }

        public async Task<IActionResult> PaymentSuccessfull(string firstName, string lastName, string email, string address, string phoneNumber, string city, string state, string postalCode, string country)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                RedirectToAction("Index", "Home");
            }

            var cart = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, includeProperties: "CartItems");

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Address = address,
                PhoneNumber = phoneNumber,
                City = city,
                State = state,
                PostalCode = postalCode,
                Country = country,
                IsPaymentDone = true,
                CreatedAt = DateTime.Now,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.DiscountPrice,
                    ModeOfLecture = ci.ModeOfLecture,
                    Attempt = ci.Attempt,
                    ValidityInMonths = ci.ValidityInMonths,
                    Views = ci.Views
                }).ToList()
            };

            await _unitOfWork.Order.AddAsync(order);
            _unitOfWork.Save();

            await _unitOfWork.Cart.DeleteAsync(cart);
            _unitOfWork.Save();

            TempData["success"] = "Order placed successfully!";
            return RedirectToAction("Success", "Checkout");
        }

        private string GenerateHash(Dictionary<string, string> parameters, string salt)
        {
            string hashString = parameters["key"] + "|" +
                                parameters["txnid"] + "|" +
                                parameters["amount"] + "|" +
                                parameters["productinfo"] + "|" +
                                parameters["firstname"] + "|" +
                                parameters["email"] + "|||||||||||" + salt;

            using (var sha512 = new System.Security.Cryptography.SHA512Managed())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(hashString);
                var hash = sha512.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

    }
}
