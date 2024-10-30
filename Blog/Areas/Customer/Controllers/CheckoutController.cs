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
        private readonly IEmailService _emailService;

        public CheckoutController(IUniteOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, EasebuzzService easebuzzService, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _easebuzzService = easebuzzService;
            _emailService = emailService;
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
                { "surl", $"https://topclasses.in/Customer/Checkout/PaymentSuccessfull?firstName={model.FirstName}&lastName={model.LastName}&email={model.Email}&address={model.Address}&phoneNumber={model.PhoneNumber}&city={model.City}&state={model.State}&postalCode={model.PostalCode}&country={model.Country}&userId={userId}" },
                { "furl", "https://topclasses.in/Customer/Checkout/Failde" }
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

        public async Task<IActionResult> PaymentSuccessfull(string firstName, string lastName, string email, string address, string phoneNumber, string city, string state, string postalCode, string country, string userId)
        {
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


            _emailService.SendEmail(new EmailDto
            {
                To = email,
                Subject = "Order confirmation and payment details",
                Body = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n<title>Order Confirmation</title>\r\n</head>\r\n<body style=\"font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;\">\r\n  <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border: 1px solid #dddddd; border-radius: 8px;\">\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        Hi <span style=\"font-weight: bold;\">{firstName + " " + lastName}</span>,\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        Thank you for your order! Your order is currently <strong>on hold</strong> until we confirm that payment has been received.\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        After completing the payment, please share a screenshot of the payment proof with us via:\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        <ul style=\"padding-left: 20px; color: #333333;\">\r\n          <li>WhatsApp: <strong>9836832223</strong></li>\r\n          <li>Email: <strong><a href=\"mailto:topclasses10@gmail.com\" style=\"color: #1a73e8; text-decoration: none;\">topclasses10@gmail.com</a></strong></li>\r\n        </ul>\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        In your message, kindly include:\r\n        <ul style=\"padding-left: 20px; color: #333333;\">\r\n          <li>Your full name</li>\r\n          <li>The course you've opted for</li>\r\n          <li>Your contact number</li>\r\n        </ul>\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        We look forward to assisting you soon!\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 14px; color: #777777;\">\r\n        Regards,<br>\r\n        The Team Topclass\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n",
            });

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
