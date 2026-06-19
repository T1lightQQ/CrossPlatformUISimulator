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

    public class VectorSvgRenderingStrategy : IRenderingStrategy
    {
        public string StrategyName => "VectorSvg";
        public void DrawBackground(Rectangle rect, Color fill) { }
        public void DrawBorder(Rectangle rect, Color stroke, float thickness) { }
        public void DrawText(string text, FontMetrics font, Point position, Color color) { }
        public bool HitTest(Rectangle bounds, Point cursor) => true;
        public void DisposeResources() { }
    }

    public class LegacyGraphicsEngine
    {
        public void InitializeRawContext(IntPtr windowHandle) { }
        public void DrawNativeButton(int x, int y, int width, int height, string legacyLabel) { }
        public void RenderTextRaster(string fontName, int size, int r, int g, int b, int x, int y, string text) { }
        public void ShowModalWindow(IntPtr parent, string title, bool blockInput) { }
    }

    public class LegacyEngineRenderingAdapter : IRenderingStrategy
    {
        private readonly LegacyGraphicsEngine _legacyEngine = new();
        public string StrategyName => "LegacyAdapter";

        public void DrawBackground(Rectangle rect, Color fill) { }
        public void DrawBorder(Rectangle rect, Color stroke, float thickness) { }
        public void DrawText(string text, FontMetrics font, Point position, Color color)
        {
            _legacyEngine.RenderTextRaster(font.Name, font.Size, color.R, color.G, color.B, position.X, position.Y, text);
        }
        public bool HitTest(Rectangle bounds, Point cursor) => true;
        public void DisposeResources() { }
    }
}