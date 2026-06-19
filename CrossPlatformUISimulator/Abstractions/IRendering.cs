using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    
    public interface IRenderingContext { }

    
    public interface IRenderingStrategy
    {
        string StrategyName { get; }
        void DrawBackground(Rectangle rect, Color fill);
    }

    
    public interface IUIStyleFlyweight
    {
        FontMetrics Font { get; }
        ColorPalette Palette { get; }
    }
}