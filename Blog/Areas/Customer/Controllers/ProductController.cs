using Blog.DataAccess.Repository;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.VM;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
        public ProductController(IUniteOfWork uniteOfWork)
        {
            _uniteOfWork = uniteOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var productList = await _uniteOfWork.Product.GetAllAsync(includeProperties: "Category,Faculty");
            var categoryList = await _uniteOfWork.Category.GetAllAsync();
            var facultyList = await  _uniteOfWork.Faculty.GetAllAsync();
            var subjectList = await _uniteOfWork.Subject.GetAllAsync();  

            var productVm = new ProductVM
            {
                CategoryList = categoryList.ToList(),
                FacultyList = facultyList.ToList(),
                ProductList = productList.ToList(),
                SubjectList = subjectList.ToList(),
            };

            return View(productVm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _uniteOfWork.Product.GetAsync(p => p.Id  == id, includeProperties: "ProductAttributes,Category,Faculty,Subject");

            if(product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [HttpPost]
        public async Task<IActionResult> CalculatePrice([FromBody] PriceCalculationRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request.");
            }

            string modeOfLectureNormalized = request.ModeOfLecture.Trim().ToLower();

            var price = await _uniteOfWork.ProductPrice
                .GetAsync(p =>
                    p.ProductId == request.ProductId &&
                    p.ModeOfLecture.Trim().ToLower() == modeOfLectureNormalized &&
                    p.ValidityInMonths == request.ValidityInMonths &&
                    p.Views == request.Views);


            if (price != null)
            {
                var result = new
                {
                    Price = price.Price,
                    DiscountPrice = price.DiscountPrice
                };
                return Ok(result);
            }

            return NotFound("Price not found for the specified parameters.");
        }

    }
}