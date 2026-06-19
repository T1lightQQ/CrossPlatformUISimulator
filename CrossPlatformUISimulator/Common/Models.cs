using System;
using System.Collections.Generic;

namespace CrossPlatformUISimulator.Common
{
    public record Point(int X, int Y);
    public record Rectangle(int X, int Y, int Width, int Height);
    public record Color(byte R, byte G, byte B);
    public record FontMetrics(string Name, int Size);
    public record ColorPalette(Color Background, Color Border, Color Text);

    public record struct StyleKey(string FontName, int FontSize, byte R, byte G, byte B) : IEquatable<StyleKey>;
    public record UIEvent(Guid SenderId, DateTime Timestamp, string EventType, object? Payload);
    public record Token(TokenType Type, string Value, int Position);
    public record UIStateChangeData(string StateType, string OldValue, string NewValue, DateTime Timestamp);
    public record ExtrinsicComponentState(Rectangle Bounds, string Text, bool Enabled, StyleKey Style, string StateName);

    
    public readonly record struct LayoutContext(int Padding, int Spacing, int AvailableWidth, int AvailableHeight, double DpiScale);

    
    public record UIContext
    {
        public required DateTime Timestamp { get; init; }
        public required string ExecutorName { get; init; }
        public bool IsDebugMode { get; init; }
        public Dictionary<string, object> SharedMetrics { get; init; } = new();
    }

    public record DialogPreset
    {
        public required string Title { get; init; }
        public required Rectangle Bounds { get; init; }
        public StyleKey Style { get; init; }
    }

    #region Системные Исключения (Exceptions)

    public class MementoIncompatibleException : Exception
    {
        public MementoIncompatibleException(string message)
            : base($"[Memento Error] Несовместимый снапшот: {message}") { }
    }

    public class ParseException : Exception
    {
        public int Position { get; }
        public ParseException(string message, int position)
            : base($"[DSL Compile Error] {message} на позиции {position}") => Position = position;
    }

    
    public record MetricsReport(int TotalNodes, int ProxyCount, int MaterializedProxies, int DistinctStylesCount);
    public record AccessibleNode(string Id, string Role, string Name, bool IsFocusable);
    #endregion
}