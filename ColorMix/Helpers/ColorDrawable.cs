using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMix.Helpers
{
    public class ColorDrawable: IDrawable
    {
        private Color _color = Colors.White;

        public void SetColor(Color color)
        {
            _color = color;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = _color;
            canvas.FillRectangle(dirtyRect);
        }
    }
}
