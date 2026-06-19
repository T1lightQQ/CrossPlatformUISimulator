using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    // Маркер контекста отрисовки (GDI+, OpenGL, Vulkan и т.д.)
    public interface IRenderingContext { }

    // Паттерн Strategy для кастомизации отрисовки тем
    public interface IRenderingStrategy
    {
        string StrategyName { get; }
        void DrawBackground(Rectangle rect, Color fill);
    }

    // Паттерн Flyweight для разделяемых тяжелых ресурсов оформления
    public interface IUIStyleFlyweight
    {
        FontMetrics Font { get; }
        ColorPalette Palette { get; }
    }
}