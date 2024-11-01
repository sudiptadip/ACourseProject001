// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Blog.Models.Models;
using Newtonsoft.Json;
using Blog.DataAccess.Repository.IRepository;

namespace Blog.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUniteOfWork _unitOfWork;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, IHttpContextAccessor httpContextAccessor, IUniteOfWork unitOfWork)
        {
            _signInManager = signInManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }


        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // Retrieve the user’s ID after login
                    var userId = _signInManager.UserManager.Users.FirstOrDefault(u => u.Email == Input.Email)?.Id;

                    // Check if there’s a cart in the session
                    var sessionCartData = _httpContextAccessor.HttpContext.Session.GetString("CartSession");
                    if (!string.IsNullOrEmpty(sessionCartData))
                    {
                        // Deserialize session cart data
                        var sessionCart = JsonConvert.DeserializeObject<Cart>(sessionCartData);

                        // Retrieve or create the user’s database cart
                        var userCart = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, includeProperties: "CartItems") ?? new Cart
                        {
                            UserId = userId,
                            CreatedAt = DateTime.Now,
                            CartItems = new List<CartItem>()
                        };

                        foreach (var sessionCartItem in sessionCart.CartItems)
                        {
                            // Check if the item already exists in the user’s database cart
                            var existingCartItem = userCart.CartItems.FirstOrDefault(ci =>
                                ci.ProductId == sessionCartItem.ProductId &&
                                ci.ModeOfLecture == sessionCartItem.ModeOfLecture &&
                                ci.ValidityInMonths == sessionCartItem.ValidityInMonths &&
                                ci.Views == sessionCartItem.Views &&
                                ci.Attempt == sessionCartItem.Attempt);

                            if (existingCartItem != null)
                            {
                                // Update existing item’s quantity and prices
                                existingCartItem.Quantity += sessionCartItem.Quantity;
                                existingCartItem.Price += sessionCartItem.Price;
                                existingCartItem.DiscountPrice += sessionCartItem.DiscountPrice;
                            }
                            else
                            {
                                // Add new item to the user’s database cart
                                userCart.CartItems.Add(new CartItem
                                {
                                    ProductId = sessionCartItem.ProductId,
                                    ModeOfLecture = sessionCartItem.ModeOfLecture,
                                    ValidityInMonths = sessionCartItem.ValidityInMonths,
                                    Views = sessionCartItem.Views,
                                    Quantity = sessionCartItem.Quantity,
                                    Attempt = sessionCartItem.Attempt,
                                    Price = sessionCartItem.Price,
                                    DiscountPrice = sessionCartItem.DiscountPrice,
                                    CreatedAt = DateTime.Now
                                });
                            }
                        }

                        // Save changes to the database
                        if (userCart.Id == 0)
                        {
                            await _unitOfWork.Cart.AddAsync(userCart); // Add new cart if it doesn’t exist
                        }
                        else
                        {
                            _unitOfWork.Cart.Update(userCart); // Update the existing cart
                        }
                         
                        _unitOfWork.Save();

                        // Clear the session cart after transferring
                        _httpContextAccessor.HttpContext.Session.Remove("CartSession");
                    }

                    // Redirect to home if returnUrl contains "AddToCart"
                    if (returnUrl.Contains("AddToCart"))
                    {
                        returnUrl = Url.Content("~/");
                    }

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

    }
}