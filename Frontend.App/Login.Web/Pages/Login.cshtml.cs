using Login.Business.Interfaces; // Contains ILoginService
using Login.Web.Models; // Contains LoginInput
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization; // For CookieRequestCultureProvider
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization; // For IStringLocalizer

namespace Login.Web.Pages
{
    /// <summary>
    /// PageModel for the Login page. Handles user login and culture change.
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly IStringLocalizer<LoginModel> _localizer; // Service for localizing strings
        private readonly ILoginService _loginService; // Business service for handling login logic

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginModel"/> class.
        /// </summary>
        /// <param name="localizer">The string localizer service.</param>
        /// <param name="loginService">The login business service.</param>
        public LoginModel(IStringLocalizer<LoginModel> localizer, ILoginService loginService)
        {
            _localizer = localizer;
            _loginService = loginService;
        }

        /// <summary>
        /// Gets or sets the input model containing user login credentials.
        /// This property is bound to the login form data.
        /// </summary>
        [BindProperty]
        public LoginInput Input { get; set; } = new LoginInput(); // Initialize to avoid null reference on GET if form is complex

        /// <summary>
        /// Gets or sets an error message to be displayed on the page if login fails or other errors occur.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Handles GET requests for the Login page.
        /// If the user is already authenticated, redirects to the Index page.
        /// </summary>
        /// <returns>The PageResult to render the login page or a RedirectToPageResult.</returns>
        public IActionResult OnGet()
        {
            // Check if the user is already authenticated (e.g., has a valid session/cookie)
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // If authenticated, redirect to the main index page
                return RedirectToPage("/Index");
            }
            // Otherwise, display the login page
            return Page();
        }

        /// <summary>
        /// Handles POST requests from the login form submission.
        /// Validates input, attempts to authenticate the user, and sets an authentication cookie on success.
        /// </summary>
        /// <returns>A RedirectToPageResult to the Index page on successful login, or redisplays the Login page with errors on failure.</returns>
        public async Task<IActionResult> OnPost()
        {
            // Check if the submitted form data is valid based on model annotations (e.g., [Required])
            if (!ModelState.IsValid)
            {
                ErrorMessage = _localizer["Your input is not validated."]; // Use localizer for messages
                ModelState.AddModelError(string.Empty, ErrorMessage);
                return Page(); // Redisplay the page with validation errors
            }

            try
            {
                // Attempt to log in the user using the provided username and password via the login service
                // The login service is expected to communicate with the backend authentication API.
                var response = await _loginService.LoginUser(Input.Username, Input.Password);

                // On successful login, append an "access_token" cookie to the HTTP response.
                // This cookie will be used for authenticating subsequent requests.
                Response.Cookies.Append("access_token", response.AccessToken, new CookieOptions
                {
                    HttpOnly = true,  // Makes the cookie inaccessible to client-side JavaScript (security measure)
                    Secure = true,    // Transmit the cookie only over HTTPS
                    SameSite = SameSiteMode.Strict, // Restricts cookie sending to first-party context only (CSRF protection)
                    Expires = DateTimeOffset.UtcNow.AddHours(1) // Set cookie expiration (e.g., 1 hour)
                });
                // Redirect to the main index page after successful login
                return RedirectToPage("/Index");
            }
            catch (UnauthorizedAccessException aex) // Catch specific exception for invalid credentials
            {
                ErrorMessage = aex.Message; // Typically, "Invalid credentials" or similar from the service
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            catch (Exception ex) // Catch any other exceptions during the login process
            {
                // Log the detailed exception ex here for debugging purposes (not shown)
                ErrorMessage = _localizer["Failed to login, please contact support."]; // Generic error message
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // If login fails or an error occurs, redisplay the login page with the error message
            return Page();
        }

        /// <summary>
        /// Handles GET requests to change the current UI culture.
        /// Sets a culture cookie and redirects back to the appropriate page.
        /// </summary>
        /// <param name="culture">The culture string to set (e.g., "en", "ar").</param>
        /// <returns>A RedirectToPageResult to either Index (if authenticated) or Login page.</returns>
        public IActionResult OnGetChangeCulture(string culture)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                // Set a cookie that stores the user's preferred culture.
                // The CookieRequestCultureProvider middleware will use this cookie to set the request culture.
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName, // Standard cookie name for culture preference
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)), // Helper to create cookie value
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) } // Cookie persistence
                );
            }

            // Redirect back to the Index page if authenticated, otherwise back to the Login page.
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToPage("/Index");
            return RedirectToPage("/Login");
        }
    }
}
