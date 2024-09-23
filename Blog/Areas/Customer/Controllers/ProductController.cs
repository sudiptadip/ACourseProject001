using Blog.DataAccess.Repository;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.VM;
using Blog.Utility.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
        private readonly ViewRenderingService _viewRenderingService;
        public ProductController(IUniteOfWork uniteOfWork, ViewRenderingService viewRenderingService)
        {
            _uniteOfWork = uniteOfWork;
            _viewRenderingService = viewRenderingService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 2; 

            var productList = await _uniteOfWork.Product.GetAllAsync(includeProperties: "Category,Faculty");
            var categoryList = await _uniteOfWork.Category.GetAllAsync();
            var facultyList = await _uniteOfWork.Faculty.GetAllAsync();
            var subjectList = await _uniteOfWork.Subject.GetAllAsync();

            int totalItems = productList.Count();
            var pagedProducts = productList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var productVm = new ProductVM
            {
                CategoryList = categoryList.ToList(),
                FacultyList = facultyList.ToList(),
                ProductList = pagedProducts,
                SubjectList = subjectList.ToList(),
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

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

        [HttpPost]
        public async Task<IActionResult> FilterProducts(List<int> categories, List<int> faculties, List<int> subjects, int page = 1)
        {
            int pageSize = 2; 

            var filteredProducts = await _uniteOfWork.Product.GetAllAsync(
                p => (categories.Count == 0 || categories.Contains(p.CategoryId)) &&
                     (faculties.Count == 0 || faculties.Contains(p.FacultyId)) &&
                     (subjects.Count == 0 || subjects.Contains(p.SubjectId)),
                includeProperties: "Category,Faculty,Subject");

            int totalItems = filteredProducts.Count();
            var pagedProducts = filteredProducts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            string productsHtml = await _viewRenderingService.RenderViewToStringAsync(ControllerContext, "_ProductListPartial", pagedProducts);

            string paginationHtml = GeneratePaginationHtml(page, totalPages);

            return Json(new { productsHtml = productsHtml, paginationHtml = paginationHtml });
        }

        private string GeneratePaginationHtml(int currentPage, int totalPages)
        {
            var pagination = new StringBuilder();

            pagination.Append("<ul class='edu-pagination'>");

            if (currentPage > 1)
            {
                pagination.Append($"<li><a href='#' data-page='{currentPage - 1}' aria-label='Previous'><i class='icon-west'></i></a></li>");
            }

            // Page number links
            for (int i = 1; i <= totalPages; i++)
            {
                if (i == currentPage)
                {
                    pagination.Append($"<li class='active'><a href='#' data-page='{i}'>{i}</a></li>");
                }
                else
                {
                    pagination.Append($"<li><a href='#' data-page='{i}'>{i}</a></li>");
                }
            }

            // Next page link
            if (currentPage < totalPages)
            {
                pagination.Append($"<li><a href='#' data-page='{currentPage + 1}' aria-label='Next'><i class='icon-east'></i></a></li>");
            }

            pagination.Append("</ul>");

            return pagination.ToString();
        }

    }
}