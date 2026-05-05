using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameReviews.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public int GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public virtual Game? Game { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;
    }
}
