using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BoardGameReviews.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [StringLength(11, MinimumLength = 11)]
        [RegularExpression("^[0-9]{11}$", ErrorMessage = "OIB must contain exactly 11 digits.")]
        public string OIB { get; set; } = string.Empty;

        [Required]
        [StringLength(13, MinimumLength = 13)]
        [RegularExpression("^[0-9]{13}$", ErrorMessage = "JMBG must contain exactly 13 digits.")]
        public string JMBG { get; set; } = string.Empty;
    }
}
