using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BoardGameReviews.Models
{
    public class Publisher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Country { get; set; }

        public virtual ICollection<Game> Games { get; set; } = new HashSet<Game>();
    }
}
