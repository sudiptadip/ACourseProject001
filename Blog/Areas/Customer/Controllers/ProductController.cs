using Blog.DataAccess.Data;
using Blog.DataAccess.Repository;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.Models;
using Blog.Models.VM;
using Blog.Utility.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Blog.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
        private readonly ViewRenderingService _viewRenderingService;
        private readonly ApplicationDbContext _context;
        public ProductController(IUniteOfWork uniteOfWork, ViewRenderingService viewRenderingService, ApplicationDbContext context)
        {
            _uniteOfWork = uniteOfWork;
            _viewRenderingService = viewRenderingService;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? productCategory = null)
        {
            int pageSize = 16;

            IEnumerable<Product> productList;
            IEnumerable<Category> categoryList;

            if (productCategory != null && productCategory.ToUpper() == "CA")
            {
                productList = await _uniteOfWork.Product.GetAllAsync(p => p.IsActive == true, includeProperties: "Category,Faculty");
                productList = productList.Where(p => p.Category.CategoryType == "CA");
                categoryList = await _uniteOfWork.Category.GetAllAsync();
                categoryList = categoryList.Where(c => c.CategoryType == "CA");
            }
            else if (productCategory != null && productCategory.ToUpper() == "CMA")
            {
                productList = await _uniteOfWork.Product.GetAllAsync(p => p.IsActive == true, includeProperties: "Category,Faculty");
                productList = productList.Where(p => p.Category.CategoryType == "CMA");
                categoryList = await _uniteOfWork.Category.GetAllAsync();
                categoryList = categoryList.Where(c => c.CategoryType == "CMA");
            }
            else if (productCategory != null && productCategory.ToUpper() == "CMA")
            {
                productList = await _uniteOfWork.Product.GetAllAsync(p => p.IsActive == true, includeProperties: "Category,Faculty");
                productList = productList.Where(p => p.Category.CategoryType == "CS");
                categoryList = await _uniteOfWork.Category.GetAllAsync();
                categoryList = categoryList.Where(c => c.CategoryType == "CS");
            }
            else
            {
                productList = await _uniteOfWork.Product.GetAllAsync(p => p.IsActive == true, includeProperties: "Category,Faculty");
                categoryList = await _uniteOfWork.Category.GetAllAsync();
            }


            var facultyList = await _uniteOfWork.Faculty.GetAllAsync();
            facultyList = facultyList.Where(x => x.IsShowMentorPage == true).OrderBy(y => y.SortedOrder);
            var subjectList = await _uniteOfWork.Subject.GetAllAsync();

            int totalItems = productList.Count();
            var pagedProducts = productList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var productVm = new ProductVM
            {
                CategoryList = categoryList.OrderBy(c => c.SortedOrder).ToList(),
                FacultyList = facultyList.OrderBy(c => c.SortedOrder).ToList(),
                ProductList = pagedProducts,
                SubjectList = subjectList.OrderBy(c => c.SortedOrder).ToList(),
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(productVm);
        }

        public async Task<IActionResult> Details(int id)
        {

            var product = await _uniteOfWork.Product.GetAsync(p => p.Id == id, includeProperties: "ProductAttributes,Category,Faculty,Subject");

            if (product == null)
            {
                return NotFound();
            }

            ProductOption price = await _context.ProductOptions.FirstOrDefaultAsync(u => u.ProductId == product.Id);

            if (price == null)
            {
                return BadRequest();
            }

            ProductDetailsVM productDetailsVM = new()
            {
                ModeOfLecture = price.ModeOfLecture.Split(',').ToList(),
                Attempt = price.Attempt.Split(",").ToList(),
                Validity = price.Validity.Split(",").ToList(),
                Views = price.Views.Split(",").ToList(),
                Product = product
            };

            return View(productDetailsVM);
        }

        [HttpPost]
        public async Task<IActionResult> GetSubjectsByCategories(List<int> categoryIds)
        {
            if (categoryIds == null || categoryIds.Count == 0)
            {
                return Json(new List<Subject>());
            }

            var subjects = await _uniteOfWork.Subject.GetAllAsync(s => categoryIds.Contains(s.CategoryId));
            var subjectList = subjects.Select(s => new
            {
                s.Id,
                s.SubjectName
            }).ToList();

            return Json(subjectList);
        }


        [HttpPost]
        public async Task<IActionResult> CalculatePrice([FromBody] PriceCalculationRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request.");
            }

            string modeOfLectureNormalized = request.ModeOfLecture.Trim().ToLower();

            ProductCombination price = await _context.ProductCombinations
                .FirstOrDefaultAsync(p => p.ModeOfLecture.Trim().ToLower() == request.ModeOfLecture.Trim().ToLower()
                && p.Attempt.Trim().ToLower() == request.Attempt.Trim().ToLower()
                && p.Validity.Trim().ToLower() == request.ValidityInMonths.Trim().ToLower() &&
                p.Views.Trim().ToLower() == request.Views.Trim().ToLower() && p.ProductId == request.ProductId
            );

            if (price != null)
            {
                var result = new
                {
                    Price = price.Price,
                    DiscountPrice = price.DiscountPrice
                };
                return Ok(result);
            }
            else
            {
                var result = new
                {
                    Price = 0,
                    DiscountPrice = 0
                };
                return Ok(result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> FilterProducts(List<int> categories, List<int> faculties, List<int> subjects, int page = 1, string? productCategory = null)
        {
            int pageSize = 16;

            //if (categories.Count == 0 && faculties.Count == 0 && subjects.Count == 0 && page <= 1)
            //{
            //    return RedirectToAction("Index", new { page, productCategory });
            //}

            IEnumerable<Product> filteredProducts = await _uniteOfWork.Product.GetAllAsync(
                p => (categories.Count == 0 || categories.Contains(p.CategoryId)) &&
                     (faculties.Count == 0 || faculties.Contains(p.FacultyId)) &&
                     (subjects.Count == 0 || subjects.Contains(p.SubjectId)),
                includeProperties: "Category,Faculty,Subject");

            if (productCategory != null)
            {
                filteredProducts = filteredProducts.Where(p => p.Category.CategoryType == productCategory);
            }

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