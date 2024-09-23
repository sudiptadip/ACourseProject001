using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Models.VM;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CheckoutController : Controller
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CheckoutController(IUniteOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
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
                    Cart = cart
                };

                return View(checkoutViewModel);


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
                    Price = ci.Price,
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
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

    }
}
