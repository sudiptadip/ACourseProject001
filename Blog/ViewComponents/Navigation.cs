using Blog.DataAccess.Data;
using Blog.DataAccess.Repository;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Models.VM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Security.Claims;

namespace Blog.ViewComponents
{
    [ViewComponent(Name = "Navigation")]
    public class Navigation : ViewComponent
    {
        private readonly IUniteOfWork _uniteOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public Navigation(IUniteOfWork uniteOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _uniteOfWork = uniteOfWork;

            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get the user ID from the claims
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get cart details for the current user
            var cart = await _uniteOfWork.Cart.GetAsync(u => u.UserId == userId, includeProperties: "CartItems");


            IEnumerable<SosalMedia> sosalMediaLinks = await _uniteOfWork.SosalMedia.GetAllAsync() ?? new List<SosalMedia>();

            ApplicationUser user = await _uniteOfWork.ApplicationUser.GetAsync(u => u.Id == userId);

            string userName = user?.FirstName ?? "";

            // Check if user ID is missing or cart is empty
            if (string.IsNullOrEmpty(userId) || cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return View("Index", new NavigationVM()
                {
                    SosalMedia = sosalMediaLinks.FirstOrDefault(),
                    TotalCart = 0,
                    UserName = userName
                });
            }

            // Calculate total cart items
            int totalCart = cart.CartItems.Count;

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
