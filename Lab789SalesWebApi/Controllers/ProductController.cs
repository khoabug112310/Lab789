using Lab789SalesWebApi.Caching;
using Lab789SalesWebApi.Core.Dtos;
using Lab789SalesWebApi.Core.Entities;
using Lab789SalesWebApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab789SalesWebApi.Controllers
{
    [Route("api/[controller]")]
    [Route("api/Products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly SaleDbContext context;
        private readonly ICacheService cacheService;
        private const string CacheKey = "products_list";

        public ProductController(SaleDbContext context, ICacheService cacheService)
        {
            this.context = context;
            this.cacheService = cacheService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var cached = await cacheService.GetAsync<List<Product>>(CacheKey);
            if (cached != null)
            {
                return Ok(cached);
            }

            var products = await context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
            await cacheService.SetAsync(CacheKey, products, TimeSpan.FromMinutes(5));
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromForm] ProductDto dto)
        {
            string? imageUrl = null;
            var imageFile = dto.Image ?? dto.ImageUrl;
            if (imageFile != null && imageFile.Length > 0)
            {
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var allowedExtension = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowedExtension.Contains(extension))
                {
                    return BadRequest(new
                    {
                        message = "Invalid image file type. Only JPG, JPEG, PNG, and GIF are allowed."
                    });
                }
                if (imageFile.Length > 2 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        message = "Image file size exceeds the limit of 2MB."
                    });
                }
                var fileName = $"{Guid.NewGuid()}{extension}"; 
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(folder);
                var filePath = Path.Combine(folder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                imageUrl = $"/images/{fileName}";
            }

            var product = new Product
            {
                ProductName = dto.ProductName.Trim(),
                Category = dto.Category.Trim(),
                Price = dto.Price,
                Quantity = dto.Quantity,
                ImageURL = imageUrl
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Invalidate Redis cache
            await cacheService.RemoveAsync(CacheKey);

            return Ok(product);
        }
    }
}
