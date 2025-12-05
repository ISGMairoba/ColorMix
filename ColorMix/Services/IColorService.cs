/// <summary>
/// This file defines the interface for the color service.
/// An interface is like a contract - it defines WHAT methods a service must have, but not HOW they work.
/// Using interfaces makes code more flexible and testable (we can swap implementations or create mock services for testing).
/// This follows the Dependency Injection pattern - ViewModels depend on IColorService, not the concrete ColorService class.
/// </summary>
using ColorMix.Data.Entities;

namespace ColorMix.Services
{
    /// <summary>
    /// Interface defining operations for managing color data.
    /// Any class implementing this interface must provide these methods.
    /// This is registered in dependency injection (MauiProgram.cs) with ColorService as the implementation.
    /// </summary>
    public interface IColorService
    {
        /// <summary>
        /// Gets all colors from the database.
        /// </summary>
        /// <returns>A list of all colors</returns>
        Task<List<ColorEntity>> GetAllColorsAsync();
        
        /// <summary>
        /// Gets a single color by ID.
        /// </summary>
        /// <param name="id">The ID to search for</param>
        /// <returns>The color if found, null otherwise</returns>
        Task<ColorEntity?> GetColorByIdAsync(int id);
        
        /// <summary>
        /// Adds a new color to the database.
        /// </summary>
        /// <param name="color">The color to add</param>
        /// <returns>The added color with generated ID</returns>
        Task<ColorEntity> AddColorAsync(ColorEntity color);
        
        /// <summary>
        /// Updates an existing color in the database.
        /// </summary>
        /// <param name="color">The color with updated values</param>
        /// <returns>The updated color</returns>
        Task<ColorEntity> UpdateColorAsync(ColorEntity color);
        
        /// <summary>
        /// Deletes a color by ID.
        /// </summary>
        /// <param name="id">ID of the color to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteColorAsync(int id);
        
        /// <summary>
        /// Gets the total count of colors in the database.
        /// </summary>
        /// <returns>Number of colors</returns>
        Task<int> GetColorCountAsync();
        
        /// <summary>
        /// Adds multiple colors in a single operation (batch add).
        /// </summary>
        /// <param name="colors">Colors to add</param>
        Task AddColorsAsync(IEnumerable<ColorEntity> colors);
        
        /// <summary>
        /// Deletes multiple colors by their IDs (batch delete).
        /// </summary>
        /// <param name="ids">IDs of colors to delete</param>
        Task DeleteColorsAsync(IEnumerable<int> ids);
    }
}
