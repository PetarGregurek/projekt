using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BoardGameReviews.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string HashedPassword { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Country { get; set; }

        [Range(5, 120)]
        public int Age { get; set; }

        public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}
