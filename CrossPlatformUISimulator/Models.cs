using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public enum WidgetType
    {
        Button,
        Label,
        Checkbox,
        Slider,
        Panel,
        LegacyRenderer
    }

    public enum ThemeType
    {
        Fluent,
        Cupertino
    }

    public enum DecoratorType
    {
        Border,
        RenderLog,
        Cached
    }

    public record Point(int X, int Y);
    public record Rectangle(int X, int Y, int Width, int Height);
    public record Color(byte R, byte G, byte B);
    public record FontMetrics(string Name, int Size);

    public interface IRenderingContext { }
    public class DefaultRenderingContext : IRenderingContext { }

    public record RenderCacheKey(Rectangle Bounds, string ComponentId);

    public record DialogPreset
    {
        public required string Title { get; init; }
        public required Rectangle Bounds { get; init; }
        public ThemeType? Theme { get; init; }
        public DecoratorType[]? Decorators { get; init; }
    }

    public record GlobalUiSettings
    {
        public required string DefaultTheme { get; init; }
        public required bool DebugMode { get; init; }
    }

    public class WidgetConfig
    {
        public required WidgetType Type { get; init; }
        public required string Id { get; init; }
        public required Rectangle Bounds { get; init; }
    }

    public class BtnConfig
    {
        public required string Id { get; init; }
        public required string Text { get; init; }
        public required Rectangle Bounds { get; init; }
    }

    public class IconSrc
    {
        public required string Path { get; init; }
    }
}