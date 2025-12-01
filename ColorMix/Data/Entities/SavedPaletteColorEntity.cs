using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColorMix.Data.Entities
{
    public class SavedPaletteColorEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SavedPaletteId { get; set; }
        
        [ForeignKey(nameof(SavedPaletteId))]
        public virtual SavedPaletteEntity SavedPalette { get; set; }

        public string ColorName { get; set; }
        public string HexValue { get; set; }
    }
}
