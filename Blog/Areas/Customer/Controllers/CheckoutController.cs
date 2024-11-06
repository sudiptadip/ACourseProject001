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
using Mono.TextTemplating;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
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
            ApplicationUser user = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == userId);

            if (order != null)
            {
                var checkoutViewModel = new CheckoutVM
                {
                    Cart = cart,
                    Address = user.Address!,
                    City = user.City!,
                    Country = user.Country!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber!,
                    PostalCode = user.Pin!,
                    State = user.State!,
                };
                return View(checkoutViewModel);
            }
            else
            {
                var checkoutViewModel = new CheckoutVM
                {
                    Address = user.Address,
                    City = user.City,
                    Country = user.Country,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    PostalCode = user.Pin,
                    State = user.State,
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

            if (model.PaymentType == PaymentType.Bank_Transfer.ToString())
            {
                return RedirectToAction(nameof(DirectPayment), new
                {
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    email = model.Email,
                    address = model.Address,
                    phoneNumber = model.PhoneNumber,
                    city = model.City,
                    state = model.State,
                    postalCode = model.PostalCode,
                    country = model.Country,
                });
            }


            var productRows = string.Join("", carts.CartItems.Select(x =>
$"<tr><td style=\"padding: 8px; border: 1px solid #ddd;\">{x.Product.ProductName}</td>" +
$"<td style=\"padding: 8px; border: 1px solid #ddd; text-align: right;\">₹{x.DiscountPrice}</td></tr>"));

            var upiImgUrl = "https://topclasses.in/uploads/image_jirj2b55.hvi.png";
            var totalPayment = carts.CartItems.Sum(x => x.DiscountPrice).ToString();
            var productName = string.Join(", ", carts.CartItems.Select(x => x.Product.ProductName));

            _emailService.SendEmail(new EmailDto
            {
                To = model.Email,
                Subject = "Thanks for your order",
                Body = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n  <title>Course Inquiry Details</title>\r" +
                $"\n</head>\r\n<body style=\"font-family: Arial, sans-serif; color: #333333; line-height: 1.6; margin: " +
                $"0; padding: 0;\">\r\n  <table style=\"max-width: 600px; margin: 20px auto; border: 1px solid #dddddd; bord" +
                $"er-collapse: collapse;\">\r\n    <tr>\r\n      <td style=\"background-color: #004080; padding: 15px; color: " +
                $"#ffffff; text-align: center;\">\r\n        <h2 style=\"margin: 0;\">Thanks for your order</h2>\r\n      </" +
                $"td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 20px;\">\r\n        <p>Dear {model.FirstName} {model.LastName},</p>" +
                $"\r\n        <p>Thanks for your order. It’s on hold until we confirm that payment has been received.</p>" +
                $"\r\n        <p>Make your payment directly into our below-mentioned bank account or UPI QR Code. Please use your Order ID as the payment reference. Your order will not be processed until the funds have cleared in our account.</p>" +
                $"\r\n        <p><strong>Modes of Payment :</strong><br>" +
                $"👉🏻 NEFT<br>" +
                $"👉🏻 IMPS<br>" +
                $"👉🏻 UPI to Bank Account payment<br>" +
                $"👉🏻 Cash deposit at Bank Counter to the given bank account</p>" +
                $"\r\n        <p>After payment to the below account, kindly share a screenshot of the payment proof over WhatsApp to 9836832223. Also, mention your full name, the course opted for, and your contact number along with the payment screenshot.</p>" +
                $"\r\n        <h3 style=\"border-bottom: 1px solid #dddddd; padding-bottom: 5px;\">Bank Details</h3>" +
                $"\r\n        <p><strong>Account Name:</strong> TUVIJAT PROJECTS PRIVATE LIMITED<br>" +
                $"<strong>Account Number:</strong> 50260851316<br>" +
                $"<strong>Bank Name:</strong> INDIAN BANK<br>" +
                $"<strong>Branch:</strong> KOLKATA HARISH MUKHERJEE RD<br>" +
                $"<strong>IFSC Code:</strong> IDIB000H547<br>" +
                $"<strong>Branch Code:</strong> 04300<br>" +
                $"<strong>Bank Address:</strong> 87A, HARISH MUKHERJEE ROAD, KOLKATA, WEST BENGAL</p>" +
                $"\r\n        <h3 style=\"border-bottom: 1px solid #dddddd; padding-bottom: 5px;\">UPI QR Code</h3>" +
                $"\r\n        <p>Please scan the UPI QR code below to make the payment.</p>" +
                $"\r\n        <img src=\"{upiImgUrl}\" alt=\"UPI QR Code\" style=\"display: block; margin: 0 auto; width: 200px; \" />" +
                $"\r\n\r\n " +
                $"       <h3 style=\"border-bottom: 1px solid #dddddd; padding-bottom: 5px;\">Customer Information</h3>\r\n        <p><str" +
                $"ong>Name:</strong> {model.FirstName} {model.LastName}<br>\r\n           <strong>Email:</strong> <a href=\"mailto:{model.Email}\" style=\"colo" +
                $"r: #004080;\">{model.Email}</a><br>\r\n           <strong>Phone:</strong> {model.PhoneNumber}</p>\r\n\r\n        <h3 style=\"border-bottom" +
                $": 1px solid #dddddd; padding-bottom: 5px;\">Address</h3>\r\n        <p>{model.Address}<br>\r\n           {model.City}, {model.State} {model.PostalCode}<b" +
                $"r>\r\n           India</p>\r\n\r\n        <h3 style=\"border-bottom: 1px solid #dddddd; padding-bottom: 5px;\">Product Information</h" +
                $"3>\r\n        <table style=\"width: 100%; border-collapse: collapse;\">\r\n          <thead>\r\n            <tr>\r\n              <th sty" +
                $"le=\"padding: 8px; border: 1px solid #ddd; background-color: #f2f2f2;\">Product Name</th>\r\n              <th style=\"padding: 8px; border: " +
                $"1px solid #ddd; background-color: #f2f2f2;\">Price</th>\r\n            </tr>\r\n          </thead>\r\n          <tbody>{productRows}</tbody>\r\n" +
                $"        </table>\r\n\r\n        <p style=\"margin: 15px 0 0; font-weight: bold;\">Total Fees : ₹{totalPayment}</p>\r\n\r\n        <h3 style=\"bord" +
                $"er-bottom: 1px solid #dddddd; padding-bottom: 5px;\">Payment Instructions</h3>\r\n        <p>If you encounter any payment issues, please share a scr" +
                $"eenshot of the payment proof with our support team. You can contact us through:</p>\r\n        <p><strong>WhatsApp:</strong> 9836832223<br>\r\n        " +
                $"Or<br>\r\n           <strong>Email:</strong> <a href=\"mailto:topclasses10@gmail.com\" style=\"color: #004080;\">topclasses10@gmail.com</a></p>\r\n\r\n        <p>Please ensure to include your full name, the course you've opted for, and your contact number for faster assistance.</p>\r\n\r\n        <p style=\"margin-top: 20px;\">Best Regards,<br>\r\n        The Support Team</p>\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; color: #777777;\">\r\n        &copy; 2023 Top Classes | All Rights Reserved\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>",
            });

            _emailService.SendEmail(new EmailDto
            {
                To = "topclasses10@gmail.com",
                Subject = $"New Checkout Notification - Order Summary for {model.FirstName} {model.LastName}",
                Body = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>New Checkout Notification</title>\r\n</head>" +
                $"\r\n<body style=\"font-family: Arial, sans-serif; color: #333333; line-height: 1.6; padding: 20px;\">\r\n    " +
                $"<div style=\"max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; padding: 20px; border-radius: 8px; " +
                $"background-color: #f9f9f9;\">\r\n        <h2 style=\"text-align: center; color: #4CAF50;\">New Checkout Notification</h2>\r\n        " +
                $"\r\n        <p style=\"margin: 0 0 15px;\">Dear Admin,</p>\r\n        \r\n        <p style=\"margin: 0 0 15px;\">A customer has completed " +
                $"the checkout process. Here are the details of the transaction:</p>\r\n        \r\n        <h3 style=\"margin: 20px 0 10px; color: #4CAF50;\">" +
                $"Customer Information</h3>\r\n        <p style=\"margin: 5px 0;\">\r\n            <strong>Name:</strong> {model.FirstName}  {model.LastName}<br>\r\n            " +
                $"<strong>Email:</strong> <a href=\"mailto:{model.Email}\" style=\"color: #4CAF50; text-decoration: none;\">{model.Email}</a><br>\r\n            <strong>Phone:</strong> " +
                $"{model.PhoneNumber}<br>\r\n        </p>\r\n        \r\n        <h3 style=\"margin: 20px 0 10px; color: #4CAF50;\">Address</h3>\r\n        <p style=\"margin: 5px 0;" +
                $"\">\r\n            {model.Address}<br>\r\n            {model.City}, {model.State} {model.PostalCode}<br>\r\n            {model.Country}\r\n        </p>\r\n\r\n        <h3 style=\"ma" +
                $"rgin: 20px 0 10px; color: #4CAF50;\">Product Information</h3>\r\n        <table style=\"width: 100%; border-collapse: collapse;\">\r\n            " +
                $"<thead>\r\n             <tr>\r\n                    <th style=\"padding: 8px; border: 1px solid #ddd; background-color: #f2f2f2;\">Product Name</th>\r\n                    " +
                $"<th style=\"padding: 8px; border: 1px solid #ddd; background-color: #f2f2f2;\">Price</th>\r\n                </tr>\r\n            </thead>\r\n            " +
                $"<tbody>{productRows}</tbody>\r\n        </table>\r\n        <p style=\"margin: 15px 0 0; font-weight: bold;\">Total Fees : ₹{totalPayment}</p>\r\n    </div>\r\n</body>\r\n</html>"
            });



            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "txnid", Guid.NewGuid().ToString() },
                { "key", _easebuzzService.Key },
                { "amount", carts.CartItems.Sum(x => x.DiscountPrice).ToString() },
                { "firstname", model.FirstName },
                { "email", model.Email },
                { "phone", model.PhoneNumber },
                { "productinfo", "Topclasses Product" },
                { "surl", $"https://topclasses.in/Customer/Checkout/PaymentSuccessfull?firstName={model.FirstName}&lastName={model.LastName}&email={model.Email}&address={model.Address}&phoneNumber={model.PhoneNumber}&city={model.City}&state={model.State}&postalCode={model.PostalCode}&country={model.Country}&userId={userId}&type={PaymentType.Ezebuzz.ToString()}" },
                { "furl", "https://topclasses.in/Customer/Checkout/Failde" }
            };

            parameters["hash"] = GenerateHash(parameters, _easebuzzService.Salt);

            string result = _easebuzzService.InitiatePayment(parameters);

            PaymentJsonResponse res = JsonConvert.DeserializeObject<PaymentJsonResponse>(result);

            return Redirect($"https://pay.easebuzz.in/pay/" + res.data);

        }

        [Authorize]
        public async Task<IActionResult> DirectPayment(string firstName, string lastName, string email, string address, string phoneNumber, string city, string state, string postalCode, string country)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var carts = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, includeProperties: "CartItems.Product");

            if(carts == null)
            {
                return RedirectToAction("Index", "Home", new { Area = "Customer" });
            }

            var totalPayment = carts.CartItems.Sum(x => x.DiscountPrice).ToString();
            var productName = string.Join(", ", carts.CartItems.Select(x => x.Product.ProductName));

            TempData["firstName"] = firstName;
            TempData["lastName"] = lastName;
            TempData["email"] = email;
            TempData["phoneNumber"] = phoneNumber;
            TempData["address"] = address;
            TempData["city"] = city;
            TempData["state"] = state;
            TempData["postalCode"] = postalCode;
            TempData["country"] = country;
            TempData["totalPayment"] = totalPayment;


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
                CreatedAt = DateTime.Now,
                OrderItems = carts.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.DiscountPrice,
                    IsPaymentDone = false,
                    ModeOfLecture = ci.ModeOfLecture,
                    Attempt = ci.Attempt,
                    ValidityInMonths = ci.ValidityInMonths,
                    Views = ci.Views
                }).ToList()
            };

            await _unitOfWork.Order.AddAsync(order);
            _unitOfWork.Save();

            await _unitOfWork.Cart.DeleteAsync(carts);
            _unitOfWork.Save();


            var productRows = string.Join("", carts.CartItems.Select(x =>
$"<tr><td style=\"padding: 8px; border: 1px solid #ddd;\">{x.Product.ProductName}</td>" +
$"<td style=\"padding: 8px; border: 1px solid #ddd; text-align: right;\">₹{x.DiscountPrice}</td></tr>"));

            var upiImgUrl = "https://topclasses.in/uploads/image_jirj2b55.hvi.png";


            _emailService.SendEmail(new EmailDto
            {
                To = email,
                Subject = "Thanks for your order",
                Body = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n  <title>Course Inquiry Details</title>\r" +
                $"\n</head>\r\n<body style=\"font-family: Arial, sans-serif; color: #333333; line-height: 1.6; margin: " +
                $"0; padding: 0;\">\r\n  <table style=\"max-width: 600px; margin: 20px auto; border: 1px solid #dddddd; bord" +
                $"er-collapse: collapse;\">\r\n    <tr>\r\n      <td style=\"background-color: #004080; padding: 15px; color: " +
                $"#ffffff; text-align: center;\">\r\n        <h2 style=\"margin: 0;\">Thanks for your order</h2>\r\n      </" +
                $"td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 20px;\">\r\n        <p>Dear {firstName} {lastName},</p>" +
                $"\r\n        <p>Thanks for your order. It’s on hold until we confirm that payment has been received.</p>" +
                $"\r\n        <p>Make your payment directly into our below-mentioned bank account or UPI QR Code. Please use your Order ID as the payment reference. Your order will not be processed until the funds have cleared in our account.</p>" +
                $"\r\n        <p><strong>Modes of Payment :</strong><br>" +
                $"👉🏻 NEFT<br>" +
                $"👉🏻 IMPS<br>" +
                $"👉🏻 UPI to Bank Account payment<br>" +
                $"👉🏻 Cash deposit at Bank Counter to the given bank account</p>" +
                $"\r\n        <p>After payment to the below account, kindly share a screenshot of the payment proof over WhatsApp to 9836832223. Also, mention your full name, the course opted for, and your contact number along with the payment screenshot.</p>" +
                $"\r\n        <h3 style=\"border-bottom: 1px solid #dddddd; padding-bottom: 5px;\">Bank Details</h3>" +
                $"\r\n        <p><strong>Account Name:</strong> TUVIJAT PROJECTS PRIVATE LIMITED<br>" +
                $"<strong>Account Number:</strong> 50260851316<br>" +
                $"<strong>Bank Name:</strong> INDIAN BANK<br>" +
                $"<strong>Branch:</strong> KOLKATA HARISH MUKHERJEE RD<br>" +
                $"<strong>IFSC Code:</strong> IDIB000H547<br>" +
                $"<strong>Branch Code:</strong> 04300<br>" +
                $"<strong>Bank Address:</strong> 87A, HARISH MUKHERJEE ROAD, KOLKATA, WEST BENGAL</p>" +
                $"\r\n        <h3 style=\"border-bottom: 1px solid #dddddd; padding-bottom: 5px;\">UPI QR Code</h3>" +
                $"\r\n        <p>Please scan the UPI QR code below to make the payment.</p>" +
                $"\r\n        <img src=\"{upiImgUrl}\" alt=\"UPI QR Code\" style=\"display: block; margin: 0 auto; width: 200px; \" />" +
                $"\r\n\r\n " +
                $"       <h3 style=\"border-bottom: 1px solid #dddddd; padding-bottom: 5px;\">Customer Information</h3>\r\n        <p><str" +
                $"ong>Name:</strong> {firstName} {lastName}<br>\r\n           <strong>Email:</strong> <a href=\"mailto:{email}\" style=\"colo" +
                $"r: #004080;\">{email}</a><br>\r\n           <strong>Phone:</strong> {phoneNumber}</p>\r\n\r\n        <h3 style=\"border-bottom" +
                $": 1px solid #dddddd; padding-bottom: 5px;\">Address</h3>\r\n        <p>{address}<br>\r\n           {city}, {state} {postalCode}<b" +
                $"r>\r\n           India</p>\r\n\r\n        <h3 style=\"border-bottom: 1px solid #dddddd; padding-bottom: 5px;\">Product Information</h" +
                $"3>\r\n        <table style=\"width: 100%; border-collapse: collapse;\">\r\n          <thead>\r\n            <tr>\r\n              <th sty" +
                $"le=\"padding: 8px; border: 1px solid #ddd; background-color: #f2f2f2;\">Product Name</th>\r\n              <th style=\"padding: 8px; border: " +
                $"1px solid #ddd; background-color: #f2f2f2;\">Price</th>\r\n            </tr>\r\n          </thead>\r\n          <tbody>{productRows}</tbody>\r\n" +
                $"        </table>\r\n\r\n        <p style=\"margin: 15px 0 0; font-weight: bold;\">Total Fees : ₹{totalPayment}</p>\r\n\r\n        <h3 style=\"bord" +
                $"er-bottom: 1px solid #dddddd; padding-bottom: 5px;\">Payment Instructions</h3>\r\n        <p>If you encounter any payment issues, please share a scr" +
                $"eenshot of the payment proof with our support team. You can contact us through:</p>\r\n        <p><strong>WhatsApp:</strong> 9836832223<br>\r\n        " +
                $"Or<br>\r\n           <strong>Email:</strong> <a href=\"mailto:topclasses10@gmail.com\" style=\"color: #004080;\">topclasses10@gmail.com</a></p>\r\n\r\n        <p>Please ensure to include your full name, the course you've opted for, and your contact number for faster assistance.</p>\r\n\r\n        <p style=\"margin-top: 20px;\">Best Regards,<br>\r\n        The Support Team</p>\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; color: #777777;\">\r\n        &copy; 2023 Top Classes | All Rights Reserved\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>",
            });

            _emailService.SendEmail(new EmailDto
            {
                To = "topclasses10@gmail.com",
                Subject = $"New Checkout Notification - Order Summary for {firstName} {lastName}",
                Body = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title>New Checkout Notification</title>\r\n</head>" +
                $"\r\n<body style=\"font-family: Arial, sans-serif; color: #333333; line-height: 1.6; padding: 20px;\">\r\n    " +
                $"<div style=\"max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; padding: 20px; border-radius: 8px; " +
                $"background-color: #f9f9f9;\">\r\n        <h2 style=\"text-align: center; color: #4CAF50;\">New Checkout Notification</h2>\r\n        " +
                $"\r\n        <p style=\"margin: 0 0 15px;\">Dear Admin,</p>\r\n        \r\n        <p style=\"margin: 0 0 15px;\">A customer has completed " +
                $"the checkout process. Here are the details of the transaction:</p>\r\n        \r\n        <h3 style=\"margin: 20px 0 10px; color: #4CAF50;\">" +
                $"Customer Information</h3>\r\n        <p style=\"margin: 5px 0;\">\r\n            <strong>Name:</strong> {firstName}  {lastName}<br>\r\n            " +
                $"<strong>Email:</strong> <a href=\"mailto:{email}\" style=\"color: #4CAF50; text-decoration: none;\">{email}</a><br>\r\n            <strong>Phone:</strong> " +
                $"{phoneNumber}<br>\r\n        </p>\r\n        \r\n        <h3 style=\"margin: 20px 0 10px; color: #4CAF50;\">Address</h3>\r\n        <p style=\"margin: 5px 0;" +
                $"\">\r\n            {address}<br>\r\n            {city}, {state} {postalCode}<br>\r\n            {country}\r\n        </p>\r\n\r\n        <h3 style=\"ma" +
                $"rgin: 20px 0 10px; color: #4CAF50;\">Product Information</h3>\r\n        <table style=\"width: 100%; border-collapse: collapse;\">\r\n            " +
                $"<thead>\r\n             <tr>\r\n                    <th style=\"padding: 8px; border: 1px solid #ddd; background-color: #f2f2f2;\">Product Name</th>\r\n                    " +
                $"<th style=\"padding: 8px; border: 1px solid #ddd; background-color: #f2f2f2;\">Price</th>\r\n                </tr>\r\n            </thead>\r\n            " +
                $"<tbody>{productRows}</tbody>\r\n        </table>\r\n        <p style=\"margin: 15px 0 0; font-weight: bold;\">Total Fees : ₹{totalPayment}</p>\r\n    </div>\r\n</body>\r\n</html>"
            });


            return View();
        }


        [Authorize]
        public async Task<IActionResult> ProcessPayment(IsConfirmPaymentDto model)
        {

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!model.isPaid)
            {
                return RedirectToAction(nameof(DirectPayment), new
                {
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    email = model.Email,
                    address = model.Address,
                    phoneNumber = model.PhoneNumber,
                    city = model.City,
                    state = model.State,
                    postalCode = model.PostalCode,
                    country = model.Country,
                });
            }
            else
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                return RedirectToAction(nameof(PaymentSuccessfull), new
                {
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    email = model.Email,
                    address = model.Address,
                    phoneNumber = model.PhoneNumber,
                    city = model.City,
                    state = model.State,
                    postalCode = model.PostalCode,
                    country = model.Country,
                    userId = userId,
                    type = PaymentType.Bank_Transfer.ToString(),
                });
            }
        }


        public async Task<IActionResult> Failde()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Success()
        {
            return View();
        }

        public async Task<IActionResult> PaymentSuccessfull(string firstName, string lastName, string email, string address, string phoneNumber, string city, string state, string postalCode, string country, string userId, string type)
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
                CreatedAt = DateTime.Now,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.DiscountPrice,
                    IsPaymentDone = false,
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


            //_emailService.SendEmail(new EmailDto
            //{
            //    To = email,
            //    Subject = "Order confirmation and payment details",
            //    Body = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n<title>Order Confirmation</title>\r\n</head>\r\n<body style=\"font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;\">\r\n  <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border: 1px solid #dddddd; border-radius: 8px;\">\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        Hi <span style=\"font-weight: bold;\">{firstName + " " + lastName}</span>,\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        Thank you for your order! Your order is currently <strong>on hold</strong> until we confirm that payment has been received.\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        After completing the payment, please share a screenshot of the payment proof with us via:\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        <ul style=\"padding-left: 20px; color: #333333;\">\r\n          <li>WhatsApp: <strong>9836832223</strong></li>\r\n          <li>Email: <strong><a href=\"mailto:topclasses10@gmail.com\" style=\"color: #1a73e8; text-decoration: none;\">topclasses10@gmail.com</a></strong></li>\r\n        </ul>\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        In your message, kindly include:\r\n        <ul style=\"padding-left: 20px; color: #333333;\">\r\n          <li>Your full name</li>\r\n          <li>The course you've opted for</li>\r\n          <li>Your contact number</li>\r\n        </ul>\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 16px; color: #333333;\">\r\n        We look forward to assisting you soon!\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td style=\"padding: 10px 0; font-size: 14px; color: #777777;\">\r\n        Regards,<br>\r\n        The Team Topclass\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n",
            //});

            TempData["success"] = "Order placed successfully!";

            if (type == PaymentType.Bank_Transfer.ToString())
            {
                return RedirectToAction("Manage", "Account", new { area = "Identity" });
            }

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
