using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public enum WidgetType { Button, Label, Checkbox, Slider, Panel }
    public enum ThemeType { Fluent, Cupertino }

    public record Point(int X, int Y);
    public record Rectangle(int X, int Y, int Width, int Height);
    public record Color(byte R, byte G, byte B);
    public record FontMetrics(string Name, int Size);
    public record ColorPalette(Color Background, Color Border, Color Text);
    public record struct StyleKey(string FontName, int FontSize, byte R, byte G, byte B) : IEquatable<StyleKey>;

    public interface IRenderingContext { }
    public class DefaultRenderingContext : IRenderingContext { }

    // Канонический базовый record события согласно техническому заданию
    public record UIEvent(Guid SenderId, DateTime Timestamp, string EventType, object? Payload);

    public record DialogPreset
    {
        public required string Title { get; init; }
        public required Rectangle Bounds { get; init; }
        public StyleKey Style { get; init; }
    }
}