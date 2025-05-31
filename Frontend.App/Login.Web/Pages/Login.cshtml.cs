using Login.Business.Interfaces;
using Login.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Login.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IStringLocalizer<LoginModel> _localizer;
        private readonly ILoginService _loginService;

        public LoginModel(IStringLocalizer<LoginModel> localizer
            , ILoginService loginService)
        {
            _localizer = localizer;
            _loginService = loginService;
        }

        [BindProperty]
        public LoginInput Input { get; set; }

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Your input is not validated.";
                ModelState.AddModelError(string.Empty, ErrorMessage);
                return Page();
            }

            try
            {
                var response = await _loginService.LoginUser(Input.Username, Input.Password);
                Response.Cookies.Append("access_token", response.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
                return RedirectToPage("/Index");
            }
            catch (UnauthorizedAccessException aex)
            {
                ErrorMessage = aex.Message;
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to login, please contact support.";
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            return Page();
        }

        public IActionResult OnGetChangeCulture(string culture)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToPage("/Index");
            return RedirectToPage("/Login");
        }
    }
}
