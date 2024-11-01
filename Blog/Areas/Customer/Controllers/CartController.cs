using Azure.Core;
using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "CartSession";

        public CartController(IUniteOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Cart cart;

            if (!string.IsNullOrEmpty(userId))
            {
                cart = await _unitOfWork.Cart.GetAsync(u => u.UserId == userId, includeProperties: "CartItems.Product");
                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                {
                    TempData["error"] = "Your cart is empty.";
                    cart = new Cart { CartItems = new List<CartItem>() };
                }
            }
            else
            {
                cart = GetCartFromSession();
            }

            Cart newCart = new Cart
            {
                CartItems = new List<CartItem>()
            };

            foreach (var cartItem in cart.CartItems)
            {
                cartItem.Product = await _unitOfWork.Product.GetAsync(u => u.Id == cartItem.ProductId);
                newCart.CartItems.Add(cartItem);
            }

            return View(newCart);
        }


        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var cartItem = await _unitOfWork.CartItem.GetAsync(ci => ci.Id == cartItemId);
                if (cartItem == null) return NotFound();

                await _unitOfWork.CartItem.DeleteAsync(cartItem);
                _unitOfWork.Save();
            }
            else
            {
                RemoveItemFromSessionCart(cartItemId);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var cartItem = await _unitOfWork.CartItem.GetAsync(ci => ci.Id == cartItemId);
                if (cartItem == null) return BadRequest();

                if (quantity <= 0)
                {
                    await _unitOfWork.CartItem.DeleteAsync(cartItem);
                }
                else
                {
                    var product = await _unitOfWork.Product.GetAsync(ci => ci.Id == cartItem.ProductId);

                    cartItem.Price = (cartItem.Price / cartItem.Quantity) * quantity;
                    cartItem.DiscountPrice = (cartItem.DiscountPrice / cartItem.Quantity) * quantity;
                    cartItem.Quantity = quantity;
                    _unitOfWork.CartItem.Update(cartItem);
                }
                _unitOfWork.Save();
            }
            else
            {
                UpdateSessionCartItemQuantity(cartItemId, quantity);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, string modeOfLecture, string validity, string views, string attempt)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the product exists
            var product = await _unitOfWork.Product.GetAsync(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            var productPrice = _context.ProductCombinations
                .FirstOrDefault(p => p.ModeOfLecture.Trim().ToLower() == modeOfLecture.Trim().ToLower()
                && p.Attempt.Trim().ToLower() == attempt.Trim().ToLower()
                && p.Validity.Trim().ToLower() == validity.Trim().ToLower() &&
                p.Views.Trim().ToLower() == views.Trim().ToLower() && p.ProductId == productId);

            if (productPrice == null || productPrice.Price <= 0)
            {
                TempData["error"] = "Unable to find a valid price for the selected options.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var cart = await GetOrCreateUserCart(userId);
                var existingCartItem = await _unitOfWork.CartItem.GetAsync(ci =>
                    ci.CartId == cart.Id && ci.ProductId == productId &&
                    ci.ModeOfLecture.Trim().ToLower() == modeOfLecture.Trim().ToLower() &&
                    ci.ValidityInMonths == validity && ci.Views == views &&
                    ci.Attempt.Trim().ToLower() == attempt.Trim().ToLower());

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += 1;
                    existingCartItem.Price += productPrice.Price;
                    existingCartItem.DiscountPrice += existingCartItem.DiscountPrice;
                    _unitOfWork.CartItem.Update(existingCartItem);
                }
                else
                {
                    await _unitOfWork.CartItem.AddAsync(new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = product.Id,
                        ModeOfLecture = modeOfLecture,
                        ValidityInMonths = validity,
                        Views = views,
                        Quantity = 1,
                        Attempt = attempt,
                        Price = productPrice.Price,
                        DiscountPrice = productPrice.DiscountPrice
                    });
                }
                _unitOfWork.Save();
            }
            else
            {
                AddToSessionCart(productId, productPrice, modeOfLecture, validity, views, attempt);
            }

            TempData["success"] = "Successfully added item to cart.";
            return RedirectToAction("Index", "Cart");
        }

        private Cart GetCartFromSession()
        {
            var cartData = _httpContextAccessor.HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartData)
                ? new Cart { CartItems = new List<CartItem>() }
                : JsonConvert.DeserializeObject<Cart>(cartData);
        }

        private void AddToSessionCart(int productId, ProductCombination productPrice, string modeOfLecture, string validity, string views, string attempt)
        {
            var cart = GetCartFromSession();

            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == productId && ci.ModeOfLecture == modeOfLecture &&
                ci.ValidityInMonths == validity && ci.Views == views && ci.Attempt == attempt);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
                existingItem.Price += productPrice.Price;
                existingItem.DiscountPrice += productPrice.DiscountPrice;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    Id = Guid.NewGuid().GetHashCode(), 
                    ProductId = productId,
                    ModeOfLecture = modeOfLecture,
                    ValidityInMonths = validity,
                    Views = views,
                    Quantity = 1,
                    Attempt = attempt,
                    Price = productPrice.Price,
                    DiscountPrice = productPrice.DiscountPrice
                });
            }

            _httpContextAccessor.HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }

        private async Task<Cart> GetOrCreateUserCart(string userId)
        {
            var cart = await _unitOfWork.Cart.GetAsync(u => u.UserId == userId, includeProperties: "CartItems");
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
            return cart;
        }

        private void RemoveItemFromSessionCart(int cartItemId)
        {
            var cart = GetCartFromSession();
            cart.CartItems.RemoveAll(ci => ci.Id == cartItemId);
            _httpContextAccessor.HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }

        private void UpdateSessionCartItemQuantity(int cartItemId, int quantity)
        {
            var cart = GetCartFromSession();
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null) return;

            if (quantity <= 0)
            {
                cart.CartItems.Remove(cartItem);
            }
            else
            {               
                cartItem.Price = (cartItem.Price / cartItem.Quantity) * quantity;
                cartItem.DiscountPrice = (cartItem.DiscountPrice / cartItem.Quantity) * quantity;
                cartItem.Quantity = quantity;
            }

            _httpContextAccessor.HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }
    }
}