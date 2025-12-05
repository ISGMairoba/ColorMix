/// <summary>
/// This file initializes the database with seed data.
/// The DatabaseInitializer class runs when the app starts and ensures:
/// 1. The database exists and is properly configured
/// 2. Default colors are added if the database is empty
/// This provides users with sample colors to work with immediately after installation.
/// </summary>
using ColorMix.Data;
using ColorMix.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ColorMix.Services
{
    /// <summary>
    /// Handles database initialization and seeding.
    /// This class is registered in the dependency injection container (see MauiProgram.cs)
    /// and runs automatically when the app starts.
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly ColorMixDbContext _context;

        /// <summary>
        /// Constructor - Receives the database context via dependency injection.
        /// </summary>
        /// <param name="context">The database context to use for initialization</param>
        public DatabaseInitializer(ColorMixDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initializes the database and seeds it with default data if needed.
        /// This method:
        /// 1. Ensures the database is created and up to date
        /// 2. Checks if colors already exist
        /// 3. If no colors exist, adds a set of default colors
        /// </summary>
        public async Task InitializeAsync()
        {
            // Ensure database is created and schema is up to date
            await _context.InitializeDatabaseAsync();

            // Check if we already have colors in the database
            var colorCount = await _context.Colors.CountAsync();
            if (colorCount > 0)
            {
                // Database already has colors, no need to seed
                return;
            }

            // Seed the database with default colors
            // This gives users a starter set of colors to experiment with
            var colors = new List<ColorEntity>
            {
                new ColorEntity { ColorName = "Bright Red", HexValue = "#FF0000", Red = 255, Green = 0, Blue = 0 },
                new ColorEntity { ColorName = "Lush Green", HexValue = "#008000", Red = 0, Green = 128, Blue = 0 },
                new ColorEntity { ColorName = "Deep Blue", HexValue = "#0000FF", Red = 0, Green = 0, Blue = 255 },
                new ColorEntity { ColorName = "Sunshine Yellow", HexValue = "#FFFF00", Red = 255, Green = 255, Blue = 0 },
                new ColorEntity { ColorName = "Burnt Orange", HexValue = "#FFA500", Red = 255, Green = 165, Blue = 0 },
                new ColorEntity { ColorName = "Royal Purple", HexValue = "#800080", Red = 128, Green = 0, Blue = 128 },
                new ColorEntity { ColorName = "Soft Pink", HexValue = "#FFC0CB", Red = 255, Green = 192, Blue = 203 },
                new ColorEntity { ColorName = "Earth Brown", HexValue = "#A52A2A", Red = 165, Green = 42, Blue = 42 },
                new ColorEntity { ColorName = "Cool Gray", HexValue = "#808080", Red = 128, Green = 128, Blue = 128 },
                new ColorEntity { ColorName = "Jet Black", HexValue = "#000000", Red = 0, Green = 0, Blue = 0 },
                new ColorEntity { ColorName = "Pure White", HexValue = "#FFFFFF", Red = 255, Green = 255, Blue = 255 },
                new ColorEntity { ColorName = "Ocean Cyan", HexValue = "#00FFFF", Red = 0, Green = 255, Blue = 255 },
                new ColorEntity { ColorName = "Vibrant Magenta", HexValue = "#FF00FF", Red = 255, Green = 0, Blue = 255 },
                new ColorEntity { ColorName = "Neon Lime", HexValue = "#00FF00", Red = 0, Green = 255, Blue = 0 },
                new ColorEntity { ColorName = "Calm Teal", HexValue = "#008080", Red = 0, Green = 128, Blue = 128 }
            };

            // Add all colors to the database in one operation (more efficient than adding one at a time)
            await _context.Colors.AddRangeAsync(colors);
            
            // Save changes to the database
            await _context.SaveChangesAsync();
        }
    }
}
