using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameReviews.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Comment { get; set; }

        public bool IsRecommended { get; set; }

        public DateTime CreatedAt { get; set; }

        public int GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public virtual Game? Game { get; set; }

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
