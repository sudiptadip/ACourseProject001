// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Blog.DataAccess.Repository;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Models.VM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUniteOfWork _unitOfWork;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, IUniteOfWork uniteOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = uniteOfWork;
        }
        public string Username { get; set; }
        [TempData]
        public string StatusMessage { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            //[Phone]
            //[Display(Name = "Phone number")]
            //public string PhoneNumber { get; set; }
            // public Order Order { get; set; }
            public List<OrderHistoryVM> OrderHistoryVM { get; set; }
        }

        //private async Task LoadAsync(IdentityUser user)
        //{
        //    var userName = await _userManager.GetUserNameAsync(user);
        //    var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

        //    Username = userName;

        //    Input = new InputModel
        //    {
        //        PhoneNumber = phoneNumber
        //    };
        //}

        public async Task<IActionResult> OnGetAsync()
        {
             var user = await _userManager.GetUserAsync(User);
            // if (user == null)
            // {
            //     return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            // }

            //var order = await  _uniteOfWork.Order.GetAsync(u => u.UserId == user.Id, includeProperties:"OrderItems.Product");
            // Input = new InputModel { Order = order };

            if (string.IsNullOrEmpty(user.Id))
            {
                return Unauthorized();
            }

            // Fetch orders for the logged-in user
            var orders = await _unitOfWork.Order.GetAllAsync(o => o.UserId == user.Id, includeProperties: "OrderItems.Product");

            var orderHistoryVM = orders.Select(order => new OrderHistoryVM
            {
                OrderId = order.Id,
                OrderDate = order.CreatedAt,
                TotalPrice = order.OrderItems.Sum(item => item.Price * item.Quantity),
             //   OrderStatus = order.Status,

                // Address Information
                FirstName = order.FirstName,
                LastName = order.LastName,
                Address = order.Address,
                City = order.City,
                State = order.State,
                PostalCode = order.PostalCode,
                Country = order.Country,

                OrderItems = order.OrderItems.Select(item => new OrderItemVM
                {
                    ProductName = item.Product.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    ProductImage = item.Product.ProductImageUrl,
                    Views = item.Views,
                    ValidityInMonths = item.ValidityInMonths,
                    Attempt = item.Attempt,
                    ModeOfLecture = item.ModeOfLecture,
                    ProductId = item.ProductId
                }).ToList()
            }).ToList();

            Input = new InputModel { OrderHistoryVM = orderHistoryVM };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
               // await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            //if (Input.PhoneNumber != phoneNumber)
            //{
            //    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            //    if (!setPhoneResult.Succeeded)
            //    {
            //        StatusMessage = "Unexpected error when trying to set phone number.";
            //        return RedirectToPage();
            //    }
            //}

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
