using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameReviews.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1900, 2200)]
        public int YearPublished { get; set; }

        [Range(1, 100)]
        public int MinPlayers { get; set; }

        [Range(1, 100)]
        public int MaxPlayers { get; set; }

        public Difficulty Difficulty { get; set; }

        public int GameTypeId { get; set; }

        [ForeignKey(nameof(GameTypeId))]
        public virtual GameType? GameType { get; set; }

        public int PublisherId { get; set; }

        [ForeignKey(nameof(PublisherId))]
        public virtual Publisher? Publisher { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Category? Category { get; set; }

        public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public virtual ICollection<Event> Events { get; set; } = new HashSet<Event>();
    }
}
