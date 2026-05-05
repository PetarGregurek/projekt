using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BoardGameReviews.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? AgeGroup { get; set; }

        public Difficulty Difficulty { get; set; }

        [Range(0, 100)]
        public int Popularity { get; set; }

        public virtual ICollection<Game> Games { get; set; } = new HashSet<Game>();
    }
}
