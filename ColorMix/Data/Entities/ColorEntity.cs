using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColorMix.Data.Entities
{
    public class ColorEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ColorName { get; set; } = string.Empty;

        [Required]
        [MaxLength(7)]
        public string HexValue { get; set; } = string.Empty;

        // Store RGB components for easier querying and conversion
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
