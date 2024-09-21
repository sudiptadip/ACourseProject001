using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Dto;
using Blog.Models.Models;
using Blog.Models.VM;
using Blog.Utility.Service;
using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using NuGet.Protocol.Plugins;
using SQLitePCL;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        // GET: Products
        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> products = await _context.Products.ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            // Populate Category and Faculty lists
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

            // If 'id' is provided, we're editing an existing product
            if (id.HasValue)
            {
                var product = await _context.Products.Include(p => p.ProductAttributes)
                    .FirstOrDefaultAsync(p => p.Id == id.Value);

                if (product == null)
                {
                    return NotFound();
                }

                // Populate ProductCreateDto with existing product data
                var productDto = new ProductCreateDto
                {
                    ProductName = product.ProductName,
                    CategoryId = product.CategoryId,
                    FacultyId = product.FacultyId,
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
                // Send product DTO to the view for editing
                return View(productDto);
            }

            // If 'id' is not provided, we're creating a new product
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
                    // Update the existing product
                    existingProduct.ProductName = productDto.ProductName;
                    existingProduct.CategoryId = productDto.CategoryId;
                    existingProduct.FacultyId = productDto.FacultyId;
                    existingProduct.SubjectId = productDto.SubjectId;
                    if (!string.IsNullOrEmpty(imageUrl)) existingProduct.ProductImageUrl = imageUrl;

                    // Clear the old attributes and add new ones
                    _context.ProductAttributes.RemoveRange(existingProduct.ProductAttributes);

                    var productAttributes = productDto.ProductAttribuets.Select(attr => new ProductAttribute
                    {
                        ProductId = existingProduct.Id,
                        AttributeName = attr.Name,
                        Value = attr.Value,
                        AttributeType = attr.AttributeType
                    }).ToList();

                    _context.ProductAttributes.AddRange(productAttributes);
                    await _context.SaveChangesAsync();

                    return Ok(productDto);
                }
            }
            else
            {
                // If id is null, create a new product
                var product = new Product
                {
                    ProductName = productDto.ProductName,
                    CategoryId = productDto.CategoryId,
                    FacultyId = productDto.FacultyId,
                    SubjectId = productDto.SubjectId,
                    ProductImageUrl = imageUrl,
                    CreatedOn = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var productAttributes = productDto.ProductAttribuets.Select(attr => new ProductAttribute
                {
                    ProductId = product.Id,
                    AttributeName = attr.Name,
                    Value = attr.Value,
                    AttributeType = attr.AttributeType
                }).ToList();

                _context.ProductAttributes.AddRange(productAttributes);
                await _context.SaveChangesAsync();

                return Ok(productDto);
            }

            return BadRequest("Unable to process the request.");
        }

        public async Task<IActionResult> SetPrices(int id)
        {
            var product = await _unitOfWork.Product.GetProductWithPricesAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductPriceVM
            {
                ProductId = id,
                ProductPrices = new List<ProductPriceItemVM>()
            };

            // Example: Populate all permutations
            var modesOfLecture = new List<string>
            {
                "Google Drive With E-Book",
                "Google Drive With Hard Copy",
                "Pen Drive With Hard Copy",
                "Live At Home with Hard Copy"
            };
            var validities = new List<int> { 6, 9, 12 };  
            var views = new List<int> { 1, 2, 3 };

            // Retrieve existing prices if they exist, or use default values
            var existingPrices = await _unitOfWork.ProductPrice.GetProductPricesAsync(id);

            // Handle duplicates by grouping and create a dictionary for easy lookup, ensure consistent trimming and case normalization
            var existingPricesDict = existingPrices
                .GroupBy(p => (p.ModeOfLecture.Trim().ToLower(), p.ValidityInMonths, p.Views)) // Normalize case and trim
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var mode in modesOfLecture)
            {
                foreach (var validity in validities)
                {
                    foreach (var view in views)
                    {
                        // Normalize the key in the same way for comparison
                        var key = (mode.Trim().ToLower(), validity, view);

                        // Check if the key exists in the dictionary and populate accordingly
                        if (existingPricesDict.TryGetValue(key, out var priceItem))
                        {
                            model.ProductPrices.Add(new ProductPriceItemVM
                            {
                                ModeOfLecture = mode,
                                ValidityInMonths = validity,
                                Views = view,
                                Price = priceItem.Price, // Set the existing price
                                DiscountPrice = priceItem.DiscountPrice // Set the existing discount price
                            });
                        }
                        else
                        {
                            // Default values for new entries
                            model.ProductPrices.Add(new ProductPriceItemVM
                            {
                                ModeOfLecture = mode,
                                ValidityInMonths = validity,
                                Views = view,
                                Price = 0, // Default to 0 if no existing price
                                DiscountPrice = 0 // Default to null if no discount
                            });
                        }
                    }
                }
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SetPrices(ProductPriceVM model)
        {
            if (ModelState.IsValid)
            {
                var product = await _unitOfWork.Product.GetProductWithPricesAsync(model.ProductId);
                if (product == null)
                {
                    return NotFound();
                }

                // Remove existing prices (if necessary)
                var existingPrices = await _unitOfWork.ProductPrice.GetProductPricesAsync(model.ProductId);
                await  _unitOfWork.ProductPrice.DeleteRangeAsync(existingPrices);

                // Add new prices
                foreach (var priceItem in model.ProductPrices)
                {
                    var productPrice = new ProductPrice
                    {
                        ProductId = model.ProductId,
                        ModeOfLecture = priceItem.ModeOfLecture,
                        ValidityInMonths = priceItem.ValidityInMonths,
                        Views = priceItem.Views,
                        Price = priceItem.Price,
                        DiscountPrice = priceItem.DiscountPrice
                    };

                  await  _unitOfWork.ProductPrice.AddAsync(productPrice);
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }

            // Debug validation errors (Optional, can be removed in production)
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage); // Check the error messages in the output window
            }

            return View(model);
        }

    }
}
