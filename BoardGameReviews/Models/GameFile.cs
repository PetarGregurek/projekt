using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameReviews.Models
{
    public class GameFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(260)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(260)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string RelativePath { get; set; } = string.Empty;

        [StringLength(120)]
        public string? ContentType { get; set; }

        public long SizeBytes { get; set; }

        public DateTime UploadedAt { get; set; }

        public int GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public virtual Game? Game { get; set; }
    }
}