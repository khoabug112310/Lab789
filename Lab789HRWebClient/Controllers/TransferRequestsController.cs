using Lab789HRWebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;

namespace Lab789HRWebClient.Controllers
{
    [Authorize]
    public class TransferRequestsController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl = "http://localhost:5213/api/DepartmentTransfers";
        private readonly string employeeApiUrl = "http://localhost:5213/api/Employee";

        private static readonly string[] AvailableDepartments = new[]
        {
            "IT", "HR", "Finance", "Sales", "Marketing", "Operations", "Legal", "Customer Support"
        };

        public TransferRequestsController(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        // GET: /TransferRequests
        public async Task<IActionResult> Index(string? status = null)
        {
            var list = new List<DepartmentTransferItem>();
            try
            {
                var url = string.IsNullOrEmpty(status) ? apiBaseUrl : $"{apiBaseUrl}?status={Uri.EscapeDataString(status)}";
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    list = JsonSerializer.Deserialize<List<DepartmentTransferItem>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<DepartmentTransferItem>();
                }
            }
            catch
            {
                // Fallback to empty list if HRWebApi is not reachable
            }

            ViewBag.CurrentStatus = status;
            return View(list);
        }

        // GET: /TransferRequests/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateDepartmentTransferViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /TransferRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentTransferViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var payload = new
            {
                employeeId = model.EmployeeId,
                toDepartment = model.ToDepartment,
                reason = model.Reason
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(apiBaseUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đơn xin chuyển phòng ban đã được tạo và gửi thành công!";
                return RedirectToAction(nameof(Index));
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(errorBody);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    ModelState.AddModelError("", msgProp.GetString() ?? "Lỗi khi tạo đơn chuyển.");
                }
                else
                {
                    ModelState.AddModelError("", errorBody);
                }
            }
            catch
            {
                ModelState.AddModelError("", string.IsNullOrEmpty(errorBody) ? "Không thể gửi đơn xin chuyển." : errorBody);
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /TransferRequests/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Approve(int id, string? comment)
        {
            var payload = new
            {
                isApproved = true,
                reviewComment = string.IsNullOrWhiteSpace(comment) ? "Approved" : comment
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{apiBaseUrl}/{id}/approve", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Đơn #{id} đã được PHÊ DUYỆT! Phòng ban nhân viên đã được cập nhật.";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Lỗi khi phê duyệt: {err}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /TransferRequests/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Reject(int id, string? comment)
        {
            var payload = new
            {
                isApproved = false,
                reviewComment = string.IsNullOrWhiteSpace(comment) ? "Rejected" : comment
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{apiBaseUrl}/{id}/reject", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["WarningMessage"] = $"Đơn #{id} đã bị TỪ CHỐI.";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Lỗi khi từ chối: {err}";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(CreateDepartmentTransferViewModel model)
        {
            var employees = new List<Employee>();
            try
            {
                var empResponse = await httpClient.GetAsync(employeeApiUrl);
                if (empResponse.IsSuccessStatusCode)
                {
                    var json = await empResponse.Content.ReadAsStringAsync();
                    employees = JsonSerializer.Deserialize<List<Employee>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Employee>();
                }
            }
            catch
            {
                // Fallback
            }

            model.EmployeeList = employees.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.FullName} ({e.Email}) - Phòng hiện tại: {e.Department}"
            }).ToList();

            model.DepartmentList = AvailableDepartments.Select(d => new SelectListItem
            {
                Value = d,
                Text = d
            }).ToList();
        }
    }
}
