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
        public void DrawBackground(Rectangle rect, Color fill) => Console.WriteLine($"[Растр] Заливка фона {rect}");
        public void DrawBorder(Rectangle rect, Color stroke, float thickness) => Console.WriteLine($"[Растр] Рамка {rect}, толщина {thickness}");
        public void DrawText(string text, FontMetrics font, Point position, Color color) => Console.WriteLine($"[Растр] Текст '{text}' шрифтом {font.Name}");
        public bool HitTest(Rectangle bounds, Point cursor) => true;
        public void DisposeResources() { }
    }

    public class VectorSvgRenderingStrategy : IRenderingStrategy
    {
        public string StrategyName => "VectorSvg";
        public void DrawBackground(Rectangle rect, Color fill) => Console.WriteLine($"[SVG Вектор] <rect x='{rect.X}' y='{rect.Y}' width='{rect.Width}' height='{rect.Height}' fill='rgb({fill.R},{fill.G},{fill.B})'/>");
        public void DrawBorder(Rectangle rect, Color stroke, float thickness) => Console.WriteLine($"[SVG Вектор] <rect stroke='rgb({stroke.R},{stroke.G},{stroke.B})' stroke-width='{thickness}'/>");
        public void DrawText(string text, FontMetrics font, Point position, Color color) => Console.WriteLine($"[SVG Вектор] <text x='position.X' y='position.Y'>{text}</text>");
        public bool HitTest(Rectangle bounds, Point cursor) => true;
        public void DisposeResources() { }
    }

    public class LegacyGraphicsEngine
    {
        public void InitializeRawContext(IntPtr windowHandle) { }
        public void DrawNativeButton(int x, int y, int width, int height, string legacyLabel) => Console.WriteLine($"[Legacy Engine] Кнопка: {legacyLabel} ({x}, {y})");
        public void RenderTextRaster(string fontName, int size, int r, int g, int b, int x, int y, string text) => Console.WriteLine($"[Legacy Engine] Растровый текст: {text}");
        public void ShowModalWindow(IntPtr parent, string title, bool blockInput) { }
    }

    public class LegacyEngineRenderingAdapter : IRenderingStrategy
    {
        private readonly LegacyGraphicsEngine _legacyEngine;
        public string StrategyName => "LegacyAdapter";

        public LegacyEngineRenderingAdapter()
        {
            _legacyEngine = new LegacyGraphicsEngine();
        }

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