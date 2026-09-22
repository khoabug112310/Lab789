using Lab789SalesWebClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Lab789SalesWebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly string productUrl = "http://localhost:5059/api/Product";
        private readonly string ordersUrl = "http://localhost:5059/api/Orders";
        private readonly HttpClient httpClient;

        public HomeController(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        // GET: /Home/Index (Danh mục sản phẩm)
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await httpClient.GetAsync(this.productUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return View(new List<Product>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<Product>>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return View(products ?? new List<Product>());
            }
            catch
            {
                return View(new List<Product>());
            }
        }

        // GET: /Home/CreateProduct
        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View();
        }

        // POST: /Home/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model.ProductName), "ProductName");
            form.Add(new StringContent(model.Category), "Category");
            form.Add(new StringContent(model.Price.ToString(CultureInfo.InvariantCulture)), "Price");
            form.Add(new StringContent(model.Quantity.ToString()), "Quantity");

            var imageFile = model.Image ?? model.ImageUrl;
            if (imageFile != null && imageFile.Length > 0)
            {
                var stream = imageFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                form.Add(fileContent, "Image", imageFile.FileName);
            }

            var response = await httpClient.PostAsync(this.productUrl, form);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", string.IsNullOrEmpty(errorContent) ? "Can't create product" : errorContent);
            return View(model);
        }

        // GET: /Home/Orders (Lịch sử đặt hàng)
        public async Task<IActionResult> Orders()
        {
            try
            {
                var response = await httpClient.GetAsync(this.ordersUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return View("Orders", new List<Order>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<Order>>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return View("Orders", orders ?? new List<Order>());
            }
            catch
            {
                return View("Orders", new List<Order>());
            }
        }

        // GET: /Home/Order (Tương thích ngược)
        public async Task<IActionResult> Order()
        {
            return await Orders();
        }

        // Helper nạp danh mục sản phẩm còn tồn kho cho Dropdown
        public async Task LoadProducts()
        {
            try
            {
                var response = await httpClient.GetAsync(productUrl);
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Products = new List<SelectListItem>();
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Product>();

                ViewBag.Products = products.Where(p => p.Quantity > 0).Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.ProductName} - Cost Price: {p.Price:N2} - Stock: {p.Quantity}"
                }).ToList();
            }
            catch
            {
                ViewBag.Products = new List<SelectListItem>();
            }
        }

        // GET: /Home/CreateOrder
        [HttpGet]
        public async Task<IActionResult> CreateOrder(int? productId)
        {
            await LoadProducts();
            var model = new OrderViewModel
            {
                ProductId = productId ?? 0,
                OrderDate = DateTime.Now,
                Quantity = 1
            };
            return View(model);
        }

        // POST: /Home/CreateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadProducts();
                return View(model);
            }

            var payload = new
            {
                ProductId = model.ProductId,
                OrderDate = model.OrderDate,
                Quantity = model.Quantity
            };

            var response = await httpClient.PostAsJsonAsync(ordersUrl, payload);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Orders));
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", string.IsNullOrEmpty(errorMessage) ? "Unable to create Order" : errorMessage);
            await LoadProducts();
            return View(model);
        }
    }
}
