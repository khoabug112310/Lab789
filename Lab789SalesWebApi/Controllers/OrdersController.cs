using Lab789SalesWebApi.Caching;
using Lab789SalesWebApi.Core.Dtos;
using Lab789SalesWebApi.Core.Entities;
using Lab789SalesWebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab789SalesWebApi.Controllers
{
    [Route("api/[controller]")]
    [Route("api/Order")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly SaleDbContext context;
        private readonly ICacheService cacheService;
        private const string CacheKey = "orders_list";
        private const string ProductCacheKey = "products_list";

        public OrdersController(SaleDbContext context, ICacheService cacheService)
        {
            this.context = context;
            this.cacheService = cacheService;
        }

        public class OrderResponseItem
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Amount { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cached = await cacheService.GetAsync<List<OrderResponseItem>>(CacheKey);
            if (cached != null)
            {
                return Ok(cached);
            }

            var orders = await context.Orders.AsNoTracking().Include(o => o.Product)
                .OrderByDescending(o => o.OrderDate).Select(o => new OrderResponseItem
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    ProductName = o.Product != null ? o.Product.ProductName : "Unknown",
                    OrderDate = o.OrderDate,
                    Quantity = o.Quantity,
                    Price = o.Product != null ? o.Product.Price : 0,
                    Amount = o.Quantity * (o.Product != null ? o.Product.Price : 0)
                }).ToListAsync();

            await cacheService.SetAsync(CacheKey, orders, TimeSpan.FromMinutes(5));
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderDto dto)
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }
            if (dto.Quantity > product.Quantity)
            {
                return BadRequest(new { message = "Quantity is not enough" });
            }
            var order = new Order
            {
                ProductId = dto.ProductId,
                OrderDate = dto.OrderDate == default ? DateTime.Now : dto.OrderDate,
                Quantity = dto.Quantity
            };
            product.Quantity -= dto.Quantity;
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Invalidate Redis caches for both orders and products
            await cacheService.RemoveAsync(CacheKey);
            await cacheService.RemoveAsync(ProductCacheKey);

            return Ok(new 
            {
                order.Id,
                order.ProductId,
                ProductName = product.ProductName,
                product.Price,
                order.Quantity,
                order.OrderDate,
                Amount = order.Quantity * product.Price
            });
        }
    }
}
