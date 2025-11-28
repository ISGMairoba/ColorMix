using ColorMix.Data;
using ColorMix.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ColorMix.Services
{
    public class DatabaseInitializer
    {
        private readonly ColorMixDbContext _context;

        public DatabaseInitializer(ColorMixDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync()
        {
            // Ensure database is created
            await _context.InitializeDatabaseAsync();

            // Check if we already have colors
            var colorCount = await _context.Colors.CountAsync();
            if (colorCount > 0)
            {
                // Database already seeded
                return;
            }

            // Seed the database with the hardcoded colors
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

            await _context.Colors.AddRangeAsync(colors);
            await _context.SaveChangesAsync();
        }
    }
}
