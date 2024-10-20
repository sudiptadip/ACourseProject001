using Azure.Core;
using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public CartController(IUniteOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                RedirectToAction("Product", "Index");
            }

            var cart = await _unitOfWork.Cart.GetAsync(u => u.UserId == userId, includeProperties: "CartItems.Product");

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["error"] = "Your cart is empty.";
                cart = new Cart
                {
                    CartItems = new List<CartItem>()
                };
            }

            return View(cart); 
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _unitOfWork.CartItem.GetAsync(ci => ci.Id == cartItemId);
            if (cartItem == null) return NotFound();

           await _unitOfWork.CartItem.DeleteAsync(cartItem);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _unitOfWork.CartItem.GetAsync(ci => ci.Id == cartItemId);
            if (cartItem == null) return BadRequest();

            if(quantity <= 0)
            {
                await _unitOfWork.CartItem.DeleteAsync(cartItem);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }

            cartItem.Price = (cartItem.Price / cartItem.Quantity) * quantity;
            cartItem.DiscountPrice = (cartItem.DiscountPrice / cartItem.Quantity) * quantity;
            cartItem.Quantity = quantity;           
            _unitOfWork.CartItem.Update(cartItem);
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, string modeOfLecture, string validity, string views, string attempt)
        {
            // Get the current user's ID
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Check if the product exists
            var product = await _unitOfWork.Product.GetAsync(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            // Check for the matching product price
            ProductCombination productPrice = _context.ProductCombinations
                .FirstOrDefault(p => p.ModeOfLecture.Trim().ToLower() == modeOfLecture.Trim().ToLower()
                && p.Attempt.Trim().ToLower() == attempt.Trim().ToLower()
                && p.Validity.Trim().ToLower() == validity.Trim().ToLower() &&
                p.Views.Trim().ToLower() == views.Trim().ToLower()
            );

            if (productPrice == null || productPrice.Price <= 0)
            {
                TempData["error"] = "Unable to find a valid price for the selected options.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }


            var cart = await _unitOfWork.Cart.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };

                await _unitOfWork.Cart.AddAsync(cart);

                 _unitOfWork.Save();
            }

            // Check if the same item already exists in the user's cart
            var existingCartItem = await _unitOfWork.CartItem.GetAsync(ci =>
                ci.Cart.UserId == userId && // Ensure the cart belongs to the same user
                ci.CartId == cart.Id &&
                ci.ProductId == productId &&
                ci.ModeOfLecture.Trim().ToLower() == modeOfLecture.Trim().ToLower() &&
                ci.ValidityInMonths == validity &&
                ci.Views == views &&
                ci.Attempt.Trim().ToLower() == attempt.Trim().ToLower()
            );

            if (existingCartItem != null)
            {
                // If item exists, increment quantity and update price
                existingCartItem.Quantity += 1;
               existingCartItem.Price += productPrice.Price;
                existingCartItem.DiscountPrice += existingCartItem.DiscountPrice;
                existingCartItem.CreatedAt = DateTime.Now;
                _unitOfWork.CartItem.Update(existingCartItem);
            }
            else
            {
                // Otherwise, add a new item to the cart
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    ModeOfLecture = modeOfLecture,
                    ValidityInMonths = validity,
                    Views = views,
                    Quantity = 1, 
                    Attempt = attempt,
                    Price = productPrice.Price,
                    DiscountPrice = productPrice.DiscountPrice,
                    CreatedAt = DateTime.Now,
                };

                await _unitOfWork.CartItem.AddAsync(cartItem);
            }

             _unitOfWork.Save();

            TempData["success"] = "Successfully added item to cart.";
            return RedirectToAction("Index", "Cart");
        }

    }
}