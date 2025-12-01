using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColorMix.Data.Entities
{
    public class SavedPaletteEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SavedPaletteColorEntity> Colors { get; set; } = new List<SavedPaletteColorEntity>();

        public virtual ICollection<PaletteVariantEntity> Variants { get; set; } = new List<PaletteVariantEntity>();
    }
}
