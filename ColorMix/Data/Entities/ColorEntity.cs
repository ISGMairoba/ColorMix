/// <summary>
/// This file defines a Color entity for the database.
/// In Entity Framework, an "entity" is a C# class that maps to a database table.
/// Each instance of this class represents one row in the Colors table.
/// </summary>
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColorMix.Data.Entities
{
    /// <summary>
    /// Represents a color in the database.
    /// Each ColorEntity stores information about a single color including its name,
    /// hex value, RGB components, and creation date.
    /// </summary>
    public class ColorEntity
    {
        /// <summary>
        /// Unique identifier for this color.
        /// [Key] tells Entity Framework this is the primary key.
        /// [DatabaseGenerated] means the database automatically generates this value (auto-increment).
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The name of the color (e.g., "Bright Red", "Ocean Blue").
        /// [Required] means this field must have a value (cannot be null).
        /// [MaxLength(100)] limits the name to 100 characters.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// The color's hex code (e.g., "#FF5733").
        /// Hex codes are 7 characters: # followed by 6 hexadecimal digits (RRGGBB).
        /// </summary>
        [Required]
        [MaxLength(7)]
        public string HexValue { get; set; } = string.Empty;

        /// <summary>
        /// Red component of the color (0-255).
        /// Storing RGB components separately makes querying and calculations easier.
        /// </summary>
        public int Red { get; set; }
        
        /// <summary>
        /// Green component of the color (0-255).
        /// </summary>
        public int Green { get; set; }
        
        /// <summary>
        /// Blue component of the color (0-255).
        /// </summary>
        public int Blue { get; set; }

        /// <summary>
        /// When this color was created.
        /// DateTime.UtcNow sets it to the current time in UTC when a new color is created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
