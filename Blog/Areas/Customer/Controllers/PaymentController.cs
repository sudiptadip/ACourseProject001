using Blog.DataAccess.Data;
using Blog.Models.Models;
using Blog.Models.VM;
using Blog.Utility.Service;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class PaymentController : Controller
    {
        //private readonly EasebuzzPaymentService _paymentService;
        //private readonly ApplicationDbContext _context;
        //private readonly IConfiguration _configuration;

        //public PaymentController( ApplicationDbContext context, IConfiguration configuration)
        //{
        //    _paymentService = new EasebuzzPaymentService();
        //    _context = context;
        //    _configuration = configuration;
        //}

        //[HttpPost]
        //public async Task<IActionResult> InitiatePayment(CheckoutVM model)
        //{
        //    model.PaymentModel.Hash = GenerateHash(model.PaymentModel); // Generate hash using Salt and required parameters
        //    var result = await _paymentService.InitiatePayment(model.PaymentModel);

        //    if (result != null)
        //    {
        //        // Redirect user to payment gateway or show a view with payment info
        //        return Redirect(result);
        //    }

        //    return View("Error"); // If something went wrong
        //}

        //// Hash Generation Logic
        //private string GenerateHash(PaymentModel model)
        //{
        //    var salt = _configuration["Easebuzz:Salt"];
        //    var hashSequence = $"{model.Key}|{model.TxnId}|{model.Amount}|{model.ProductInfo}|{model.FirstName}|{model.Email}|||||||||||{model.Email}";
        //    using (SHA512 sha512 = SHA512.Create())
        //    {
        //        var bytes = Encoding.UTF8.GetBytes(hashSequence);
        //        var hashBytes = sha512.ComputeHash(bytes);
        //        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        //    }
        //}

        //// Handle Payment Success Callback
        //[HttpPost]
        //public async Task<IActionResult> PaymentSuccess(string txnId, int productId)
        //{
        //    // Find the corresponding product
        //    var product = await _context.Products.FindAsync(productId);

        //    if (product != null)
        //    {
        //        // Mark the product as paid
        //        product.IsPayment = true;

        //        // Update product in the database
        //        _context.Update(product);
        //        await _context.SaveChangesAsync();
        //    }

        //    return View("Success");
        //}

        //// Handle Payment Failure Callback
        //[HttpPost]
        //public IActionResult PaymentFailure()
        //{
        //    // Handle what to do if payment fails
        //    return View("Failure");
        //}
    
    }
}
