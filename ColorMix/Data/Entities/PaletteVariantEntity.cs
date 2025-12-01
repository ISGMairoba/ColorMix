using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColorMix.Data.Entities
{
    public class PaletteVariantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string HexColor { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PaletteComponentEntity> Components { get; set; } = new List<PaletteComponentEntity>();
    }
}
