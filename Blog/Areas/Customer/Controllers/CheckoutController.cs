using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Models.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public CheckoutController(IUniteOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

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

            var model = new PaymentModel
            {
                Key = _configuration["Easebuzz:Key"],
                TxnId = Guid.NewGuid().ToString(),
                Amount = "500.00", // Example amount
                ProductInfo = "Sample Product",
                FirstName = "John",
                Email = "john@example.com",
                Phone = "9876543210",
                SuccessUrl = Url.Action("PaymentSuccess", "Payment", null, Request.Scheme),
                FailureUrl = Url.Action("PaymentFailure", "Payment", null, Request.Scheme)
            };

            var salt = _configuration["Easebuzz:Salt"];
            model.Hash = GenerateHash(model.Key, model.TxnId, model.Amount, model.ProductInfo, model.FirstName, model.Email, salt);

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
                    PaymentModel = model
                };
                return View(checkoutViewModel);
            }
            else
            {
                var checkoutViewModel = new CheckoutVM
                {
                    Cart = cart,
                    PaymentModel = model
                };

                return View(checkoutViewModel);

            }

        }

        public string GenerateHash(string key, string txnId, string amount, string productInfo, string firstName, string email, string salt)
        {
            var data = $"{key}|{txnId}|{amount}|{productInfo}|{firstName}|{email}|||||||||||{salt}";
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutVM model)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                var carts = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, includeProperties: "CartItems.Product");

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

            var cart = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, includeProperties: "CartItems");

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order
            {
                UserId = userId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Country = model.Country,
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
            return RedirectToAction("Index", "Product");
        }

    }
}
