using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public enum WidgetType { Button, Label, Checkbox, Slider, Panel }
    public enum ThemeType { Fluent, Cupertino }
    public enum DecoratorType { Border, RenderLog, Cached }

    public record Point(int X, int Y);
    public record Rectangle(int X, int Y, int Width, int Height);
    public record Color(byte R, byte G, byte B);
    public record FontMetrics(string Name, int Size);
    public record ColorPalette(Color Background, Color Border, Color Text);
    public record struct StyleKey(string FontName, int FontSize, byte R, byte G, byte B) : IEquatable<StyleKey>;

    public interface IRenderingContext { }
    public class DefaultRenderingContext : IRenderingContext { }

    public record UIEvent(string EventType, string TargetComponentId, object Payload, DateTime Timestamp);

    public record DialogPreset
    {
        public required string Title { get; init; }
        public required Rectangle Bounds { get; init; }
        public StyleKey Style { get; init; }
    }

    public class WidgetConfig
    {
        public required WidgetType Type { get; init; }
        public required string Id { get; init; }
        public required Rectangle Bounds { get; init; }
        public required StyleKey Style { get; init; }
    }

    // Перечисления лексем для UI-DSL Компилятора
    public enum TokenType
    {
        Select, Execute, Where, Arrow, Selector, Predicate, Action, EOF
    }

    public record Token(TokenType Type, string Value, int Position);
}