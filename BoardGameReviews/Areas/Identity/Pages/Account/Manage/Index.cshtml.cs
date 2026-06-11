using System.ComponentModel.DataAnnotations;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoardGameReviews.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public IndexModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string Username { get; private set; } = string.Empty;

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11)]
        [RegularExpression("^\\d{11}$", ErrorMessage = "OIB must contain exactly 11 digits.")]
        [Display(Name = "OIB")]
        public string OIB { get; set; } = string.Empty;

        [Required]
        [StringLength(13, MinimumLength = 13)]
        [RegularExpression("^\\d{13}$", ErrorMessage = "JMBG must contain exactly 13 digits.")]
        [Display(Name = "JMBG")]
        public string JMBG { get; set; } = string.Empty;
    }

    private async Task LoadAsync(AppUser user)
    {
        Username = await _userManager.GetUserNameAsync(user) ?? string.Empty;

        Input = new InputModel
        {
            PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
            OIB = user.OIB,
            JMBG = user.JMBG
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }
        }

        user.OIB = Input.OIB;
        user.JMBG = Input.JMBG;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadAsync(user);
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }
}
