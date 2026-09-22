using Lab789HRWebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Lab789HRWebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly string url = "http://localhost:5213/api/Employee";
        private readonly HttpClient httpClient;

        public HomeController(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await this.httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode || User.Identity!.IsAuthenticated != true)
                {
                    return View(new List<Employee>());
                }
                var json = await response.Content.ReadAsStringAsync();
                var employees = JsonSerializer.Deserialize<List<Employee>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return View(employees ?? new List<Employee>());
            }
            catch
            {
                return View(new List<Employee>());
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await this.httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to create employee!");
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
