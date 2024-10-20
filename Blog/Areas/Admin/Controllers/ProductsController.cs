using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.Models;
using Blog.Models.VM;
using Blog.Utility.Service;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using NuGet.Protocol.Plugins;
using SQLitePCL;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUniteOfWork _unitOfWork;

        private readonly IImageService _imageService;

        public ProductsController(ApplicationDbContext context, IImageService imageService, IUniteOfWork unitOfWork)
        {
            _context = context;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> products = await _context.Products.ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            IEnumerable<SelectListItem> categoryList = _context.Categories.Select(u => new SelectListItem
            {
                Text = u.CategoryName,
                Value = u.Id.ToString()
            }).ToList();

            IEnumerable<SelectListItem> subjectList = _context.Subjects.Select(u => new SelectListItem
            {
                Text = u.SubjectName,
                Value = u.Id.ToString()
            }).ToList();

            IEnumerable<SelectListItem> facultyList = _context.Faculties.Select(u => new SelectListItem
            {
                Text = u.FacultyName,
                Value = u.Id.ToString()
            }).ToList();

            ViewData["CategoryList"] = categoryList;
            ViewData["FacultyList"] = facultyList;
            ViewData["SubjectList"] = subjectList;

            if (id.HasValue)
            {
                var product = await _context.Products.Include(p => p.ProductAttributes)
                    .FirstOrDefaultAsync(p => p.Id == id.Value);

                if (product == null)
                {
                    return NotFound();
                }

                var productDto = new ProductCreateDto
                {
                    ProductName = product.ProductName,
                    CategoryId = product.CategoryId,
                    FacultyId = product.FacultyId,
                    DefaultPrice = product.DefaultPrice,
                    DefaultDiscountPrice = product.DefaultDiscountPrice,
                    IsActive = product.IsActive,
                    ProductDescription = product.ProductDescription,
                    SubjectId = product.SubjectId,
                    ProductImageUrl = product.ProductImageUrl,
                    ProductAttribuets = product.ProductAttributes.Select(a => new ProductAttribuetDto
                    {
                        Name = a.AttributeName,
                        Value = a.Value,
                        AttributeType = a.AttributeType
                    }).ToList()
                };

                ViewBag.ProductId = id;
                return View(productDto);
            }


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto productDto, int? id)
        {
            string imageUrl = "";

            if (productDto.ProductImage != null && productDto.ProductImage.Length > 0)
            {
                var url = await _imageService.UploadSingleImageAsync(productDto.ProductImage);
                imageUrl = url ?? throw new Exception("Image upload failed.");
            }

            // If id is not null, we're updating an existing product
            if (id.HasValue)
            {
                var existingProduct = await _context.Products.Include(p => p.ProductAttributes)
                    .FirstOrDefaultAsync(p => p.Id == id.Value);

                if (existingProduct != null)
                {
                    existingProduct.ProductName = productDto.ProductName;
                    existingProduct.CategoryId = productDto.CategoryId;
                    existingProduct.FacultyId = productDto.FacultyId;
                    existingProduct.SubjectId = productDto.SubjectId;
                    existingProduct.ProductDescription = productDto.ProductDescription;
                    existingProduct.DefaultDiscountPrice = productDto.DefaultDiscountPrice;
                    existingProduct.DefaultPrice = productDto.DefaultPrice;
                    existingProduct.IsActive = productDto.IsActive;
                    

                    if (!string.IsNullOrEmpty(imageUrl)) existingProduct.ProductImageUrl = imageUrl;

                    _context.ProductAttributes.RemoveRange(existingProduct.ProductAttributes);

                    var productAttributes = productDto.ProductAttribuets.Select(attr => new ProductAttribute
                    {
                        ProductId = existingProduct.Id,
                        AttributeName = attr.Name,
                        Value = attr.Value,
                        AttributeType = attr.AttributeType
                    }).ToList();

                    _context.ProductAttributes.AddRange(productAttributes);
                    _unitOfWork.Save();

                    return Ok(productDto);
                }
            }
            else
            {
                var product = new Product
                {
                    ProductName = productDto.ProductName,
                    CategoryId = productDto.CategoryId,
                    FacultyId = productDto.FacultyId,
                    SubjectId = productDto.SubjectId,
                    ProductImageUrl = imageUrl,
                    CreatedOn = DateTime.Now,
                    DefaultDiscountPrice = productDto.DefaultDiscountPrice,
                    DefaultPrice = productDto.DefaultPrice,
                    IsActive = productDto.IsActive,
                    ProductDescription = productDto.ProductDescription,
                };

                _context.Products.Add(product);
                _context.SaveChanges();

                var productAttributes = productDto.ProductAttribuets.Select(attr => new ProductAttribute
                {
                    ProductId = product.Id,
                    AttributeName = attr.Name,
                    Value = attr.Value,
                    AttributeType = attr.AttributeType
                }).ToList();

                _context.ProductAttributes.AddRange(productAttributes);
                _unitOfWork.Save();

                return Ok(productDto);
            }

            return BadRequest("Unable to process the request.");
        }
        

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
           var product = await _context.Products.Include(p => p.ProductAttributes).FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            _context.ProductAttributes.RemoveRange(product.ProductAttributes);

           // _context.ProductPrices.RemoveRange(product.ProductPrice);

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Product deleted successfully" });
        }


        [HttpGet]
        public async Task<IActionResult> SetPrices(int id)
        {
            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ProductOption productOptions = await _context.ProductOptions.FirstOrDefaultAsync(u => u.ProductId == id);
            ProductPriceVM priceVM = new ProductPriceVM();
            var options = new LectureOption();

            if (productOptions == null)
            {
                productOptions = new ProductOption { ProductId = id, ModeOfLecture = "", Attempt = "", Validity = "", Views = "",  };
            }

            priceVM.ProductOption = productOptions;


            var existingCombinations = await _context.ProductCombinations
                .Where(c => c.ProductId == id)
                .ToListAsync();


            if (!existingCombinations.Any())
            {
                var combinations = options.GetAllCombinations(productOptions, id, 0, 0);
                priceVM.Combinations = combinations;
            }
            else
            {
                priceVM.Combinations = existingCombinations;
            }

            return View(priceVM);
        }

        [HttpPost]
        public async Task<IActionResult> SetPrices(ProductPriceVM priceVM)
        {
            if (ModelState.IsValid)
            {
                if (priceVM.ProductOption.Id == 0)
                {
                    await _context.ProductOptions.AddAsync(priceVM.ProductOption);
                }
                else
                {
                    _context.ProductOptions.Update(priceVM.ProductOption);
                }

                await _context.SaveChangesAsync();
                TempData["success"] = "Successfully saved!";
                return RedirectToAction(nameof(SetPrices), new { id = priceVM.ProductOption.ProductId });
            }

            return View(priceVM);
        }

        [HttpPost("saveCombinations")]
        public async Task<IActionResult> SaveCombinations(List<ProductCombination> combinations)
        {
            if (combinations == null || !combinations.Any())
            {
                return BadRequest("No combinations provided.");
            }

            try
            {
                var existingCombination = _context.ProductCombinations.Where(c => c.ProductId == combinations[0].ProductId);

                if (existingCombination != null)
                {
                    _context.RemoveRange(existingCombination);
                    _context.SaveChanges();
                }

                _context.ProductCombinations.AddRange(combinations);

                await _context.SaveChangesAsync();
                TempData["success"] = "Combinations saved successfully!";
                return RedirectToAction(nameof(SetPrices), new { id = combinations.FirstOrDefault()?.ProductId });
            }
            catch (Exception ex)
            {
                TempData["success"] = ex.Message.ToString();
                return View(nameof(SetPrices), new { id = combinations.FirstOrDefault()?.ProductId });
            }


        }

    }
}


public class LectureOption
{

    public List<ProductCombination> GetAllCombinations(ProductOption option, int productId, decimal price, decimal discountPrice)
    {
        var modes = option.ModeOfLecture.Split(',').ToList();
        var validities = option.Validity.Split(',').ToList();
        var views = option.Views.Split(',').ToList();
        var attempts = option.Attempt.Split(',').ToList();

        var combinations = new List<ProductCombination>();

        foreach (var mode in modes)
        {
            foreach (var validity in validities)
            {
                foreach (var view in views)
                {
                    foreach (var attempt in attempts)
                    {
                        combinations.Add(new ProductCombination
                        {
                            ModeOfLecture = mode,
                            Validity = validity,
                            Views = view,
                            Attempt = attempt,
                            ProductId = productId,
                            Price = price,
                            DiscountPrice = discountPrice,
                        });
                    }
                }
            }
        }

        return combinations;
    }
}