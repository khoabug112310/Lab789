using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> signInManager;

        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        private static bool IsAllowReturnUrl(string? returnUrl)
        {
            if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri)) return false;
            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)) return false;

            return uri.Port is 5192 or 5247;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? "/";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email or Password is empty");
                ViewBag.ReturnUrl = returnUrl ?? "/";
                return View();
            }

            var result = await signInManager.PasswordSignInAsync(email, password,
                isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (IsAllowReturnUrl(returnUrl))
                {
                    return Redirect(returnUrl!);
                }
                return Redirect("/");
            }

            ModelState.AddModelError("", "Invalid email or password");
            ViewBag.ReturnUrl = returnUrl ?? "/";
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await signInManager.SignOutAsync();
            if (IsAllowReturnUrl(returnUrl))
            {
                return Redirect(returnUrl!);
            }
            return RedirectToAction(nameof(Login));
        }
    }
}
