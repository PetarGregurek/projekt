using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BoardGameReviews.Models
{
    public class GameType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public virtual ICollection<Game> Games { get; set; } = new HashSet<Game>();
    }
}
