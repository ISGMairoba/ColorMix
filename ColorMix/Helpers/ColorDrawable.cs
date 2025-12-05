/// <summary>
/// This file contains a helper class for drawing colored rectangles.
/// ColorDrawable is used to display color previews in the UI.
/// It implements IDrawable, which is MAUI's interface for custom graphics drawing.
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMix.Helpers
{
    /// <summary>
    /// A custom drawable that fills a rectangle with a solid color.
    /// Used in the Create Color page to show a live preview of the selected color.
    /// Implements IDrawable to integrate with MAUI's GraphicsView.
    /// </summary>
    public class ColorDrawable: IDrawable
    {
        private Color _color = Colors.White;

        /// <summary>
        /// Updates the color to display.
        /// After calling this, the GraphicsView will need to redraw (invalidate).
        /// </summary>
        /// <param name="color">The color to display</param>
        public void SetColor(Color color)
        {
            _color = color;
        }

        /// <summary>
        /// Draws the color preview.
        /// Called automatically by MAUI when the GraphicsView needs to render.
        /// Fills the entire available rectangle with the current color.
        /// </summary>
        /// <param name="canvas">The drawing canvas</param>
        /// <param name="dirtyRect">The area that needs redrawing</param>
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = _color;  // Set fill color
            canvas.FillRectangle(dirtyRect);  // Fill the rectangle
        }
    }
}
