using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoardGameReviews.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ProviderDisplayName { get; set; }
        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(11, MinimumLength = 11)]
            [RegularExpression("^[0-9]{11}$", ErrorMessage = "OIB must contain exactly 11 digits.")]
            [Display(Name = "OIB")]
            public string OIB { get; set; } = string.Empty;

            [Required]
            [StringLength(13, MinimumLength = 13)]
            [RegularExpression("^[0-9]{13}$", ErrorMessage = "JMBG must contain exactly 13 digits.")]
            [Display(Name = "JMBG")]
            public string JMBG { get; set; } = string.Empty;
        }

        // Step 1: POST from Login page — redirect to provider
        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            returnUrl = NormalizeReturnUrl(returnUrl);
            _logger.LogInformation("Starting external login challenge for provider {Provider} with returnUrl {ReturnUrl}", provider, returnUrl);

            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        // Step 2: Provider redirects back here
        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = NormalizeReturnUrl(returnUrl);
            _logger.LogInformation("External login callback reached with returnUrl {ReturnUrl}", returnUrl);

            if (remoteError != null)
            {
                _logger.LogWarning("External provider returned remote error: {RemoteError}", remoteError);
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("External login info could not be loaded in callback.");
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Try to sign in with existing external login
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("External login blocked because user is locked out.");
                return RedirectToPage("./Lockout");
            }

            // New user — show confirmation form
            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName;

            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            Input = new InputModel { Email = email };

            return Page();
        }

        // Step 3: Confirm and create account
        public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
        {
            returnUrl = NormalizeReturnUrl(returnUrl);

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("External login info missing during confirmation.");
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    OIB = Input.OIB,
                    JMBG = Input.JMBG,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    createResult = await _userManager.AddLoginAsync(user, info);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private string NormalizeReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return Url.Content("~/");
            }

            if (!Url.IsLocalUrl(returnUrl))
            {
                return Url.Content("~/");
            }

            if (returnUrl.StartsWith("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase) ||
                returnUrl.StartsWith("/Identity/Account/ExternalLogin", StringComparison.OrdinalIgnoreCase))
            {
                return Url.Content("~/");
            }

            return returnUrl;
        }
    }
}
