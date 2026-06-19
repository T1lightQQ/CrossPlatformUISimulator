using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class RasterRenderingStrategy : IRenderingStrategy
    {
        public string StrategyName => "Raster";
        public void DrawBackground(Rectangle rect, Color fill) { }
        public void DrawBorder(Rectangle rect, Color stroke, float thickness) { }
        public void DrawText(string text, FontMetrics font, Point position, Color color) { }
        public bool HitTest(Rectangle bounds, Point cursor) => true;
        public void DisposeResources() { }
    }
}