using Microsoft.EntityFrameworkCore;
using ColorMix.Data;
using ColorMix.Data.Entities;

namespace ColorMix.Services
{
    public class ColorService : IColorService
    {
        private readonly ColorMixDbContext _context;

        public ColorService(ColorMixDbContext context)
        {
            _context = context;
        }

        public async Task<List<ColorEntity>> GetAllColorsAsync()
        {
            return await _context.Colors
                .OrderBy(c => c.ColorName)
                .ToListAsync();
        }

        public async Task<ColorEntity?> GetColorByIdAsync(int id)
        {
            return await _context.Colors.FindAsync(id);
        }

        public async Task<ColorEntity> AddColorAsync(ColorEntity color)
        {
            _context.Colors.Add(color);
            await _context.SaveChangesAsync();
            return color;
        }

        public async Task<ColorEntity> UpdateColorAsync(ColorEntity color)
        {
            _context.Colors.Update(color);
            await _context.SaveChangesAsync();
            return color;
        }

        public async Task<bool> DeleteColorAsync(int id)
        {
            var color = await _context.Colors.FindAsync(id);
            if (color == null)
                return false;

            _context.Colors.Remove(color);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetColorCountAsync()
        {
            return await _context.Colors.CountAsync();
        }

        public async Task AddColorsAsync(IEnumerable<ColorEntity> colors)
        {
            await _context.Colors.AddRangeAsync(colors);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteColorsAsync(IEnumerable<int> ids)
        {
            var colorsToDelete = await _context.Colors
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            if (colorsToDelete.Any())
            {
                _context.Colors.RemoveRange(colorsToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}
