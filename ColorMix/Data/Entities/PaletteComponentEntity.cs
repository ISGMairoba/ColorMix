using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColorMix.Data.Entities
{
    public class PaletteComponentEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PaletteVariantId { get; set; }
        
        [ForeignKey(nameof(PaletteVariantId))]
        public virtual PaletteVariantEntity PaletteVariant { get; set; }

        [Required]
        public string ColorName { get; set; }

        [Required]
        public string HexColor { get; set; }

        public double Ratio { get; set; }
        public double Percentage { get; set; }
    }
}
