using Blog.DataAccess.Data;
using Blog.DataAccess.Repository;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Models.VM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Blog.ViewComponents
{
    [ViewComponent(Name = "Navigation")]
    public class Navigation : ViewComponent
    {
        private readonly IUniteOfWork _uniteOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "CartSession";

        public Navigation(IUniteOfWork uniteOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _uniteOfWork = uniteOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get the user ID from the claims
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int totalCart = 0;

            if (!string.IsNullOrEmpty(userId))
            {
                // Get cart details from the database for the logged-in user
                var cart = await _uniteOfWork.Cart.GetAsync(u => u.UserId == userId, includeProperties: "CartItems");

                // If the cart exists, get the count of items
                totalCart = cart?.CartItems?.Count ?? 0;
            }
            else
            {
                // Retrieve cart data from the session for unauthenticated users
                var sessionCartData = _httpContextAccessor.HttpContext.Session.GetString(CartSessionKey);
                if (!string.IsNullOrEmpty(sessionCartData))
                {
                    // Deserialize session cart data
                    var sessionCart = JsonConvert.DeserializeObject<Cart>(sessionCartData);
                    totalCart = sessionCart?.CartItems?.Count ?? 0;
                }
            }

            // Get social media links and user information
            IEnumerable<SosalMedia> sosalMediaLinks = await _uniteOfWork.SosalMedia.GetAllAsync() ?? new List<SosalMedia>();
            ApplicationUser user = await _uniteOfWork.ApplicationUser.GetAsync(u => u.Id == userId);
            string userName = user?.FirstName ?? "";

            // Return the view with the calculated data
            return View("Index", new NavigationVM()
            {
                SosalMedia = sosalMediaLinks.FirstOrDefault(),
                TotalCart = totalCart,
                UserName = userName
            });
        }
    }
}