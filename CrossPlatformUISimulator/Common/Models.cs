using System;
using System.Collections.Generic;

namespace CrossPlatformUISimulator.Common
{
    public record Point(int X, int Y);
    public record Rectangle(int X, int Y, int Width, int Height);
    public record Color(byte R, byte G, byte B);
    public record FontMetrics(string Name, int Size);
    public record ColorPalette(Color Background, Color Border, Color Text);

    // Ключ для Flyweight-фабрики стилей
    public record struct StyleKey(string FontName, int FontSize, byte R, byte G, byte B) : IEquatable<StyleKey>;

    // Базовый атомарный класс события для Mediator
    public record UIEvent(Guid SenderId, DateTime Timestamp, string EventType, object? Payload);

    // Единица трансляции DSL скрипта
    public record Token(TokenType Type, string Value, int Position);

    // ЧАСТЬ 28: Атомарные данные изменения состояния для паттерна Observer
    public record UIStateChangeData(string StateType, string OldValue, string NewValue, DateTime Timestamp);

    // ЧАСТЬ 29: Расширенный снимок состояния для паттерна Memento с поддержкой StateName
    public record ExtrinsicComponentState(Rectangle Bounds, string Text, bool Enabled, StyleKey Style, string StateName);

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

    #endregion
}