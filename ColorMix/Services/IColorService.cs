using ColorMix.Data.Entities;

namespace ColorMix.Services
{
    public interface IColorService
    {
        Task<List<ColorEntity>> GetAllColorsAsync();
        Task<ColorEntity?> GetColorByIdAsync(int id);
        Task<ColorEntity> AddColorAsync(ColorEntity color);
        Task<ColorEntity> UpdateColorAsync(ColorEntity color);
        Task<bool> DeleteColorAsync(int id);
        Task<int> GetColorCountAsync();
        Task AddColorsAsync(IEnumerable<ColorEntity> colors);
        Task DeleteColorsAsync(IEnumerable<int> ids);
    }
}
