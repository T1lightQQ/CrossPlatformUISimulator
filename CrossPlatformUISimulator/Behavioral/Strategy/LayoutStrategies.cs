using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Behavioral.Strategy
{
    // 1. Последовательное вертикальное размещение элементов (Stack)
    public record StackLayoutStrategy : ILayoutStrategy
    {
        public IReadOnlyDictionary<string, Rectangle> CalculateBounds(IContainerComponent container, LayoutContext context)
        {
            var result = new Dictionary<string, Rectangle>();
            int currentY = context.Padding;
            int availableWidth = context.AvailableWidth - (context.Padding * 2);

            foreach (var child in container.Children)
            {
                // Масштабируем с учётом DPI симулятора
                int calculatedHeight = (int)(child.BoundingBox.Height * context.DpiScale);
                int calculatedWidth = Math.Min((int)(child.BoundingBox.Width * context.DpiScale), availableWidth);

                result[child.Id] = new Rectangle(context.Padding, currentY, calculatedWidth, calculatedHeight);
                currentY += calculatedHeight + context.Spacing;
            }

            return result;
        }
    }

    // 2. Размещение по строкам и столбцам (Упрощенная Grid-сетка с равными весами)
    public record GridLayoutStrategy : ILayoutStrategy
    {
        private readonly int _columns;

        public GridLayoutStrategy(int columns)
        {
            _columns = columns <= 0 ? 1 : columns;
        }

        public IReadOnlyDictionary<string, Rectangle> CalculateBounds(IContainerComponent container, LayoutContext context)
        {
            var result = new Dictionary<string, Rectangle>();
            var children = container.Children;
            if (children.Count == 0) return result;

            int usableWidth = context.AvailableWidth - (context.Padding * 2);
            int cellWidth = (usableWidth - (context.Spacing * (_columns - 1))) / _columns;
            int cellHeight = 60; // Фиксированная базовая высота ячейки для тестов

            for (int i = 0; i < children.Count; i++)
            {
                int row = i / _columns;
                int col = i % _columns;

                int x = context.Padding + col * (cellWidth + context.Spacing);
                int y = context.Padding + row * (cellHeight + context.Spacing);

                result[children[i].Id] = new Rectangle(x, y, cellWidth, cellHeight);
            }

            return result;
        }
    }

    // 3. Свободное размещение по абсолютным координатам
    public record FreeFormLayoutStrategy : ILayoutStrategy
    {
        public IReadOnlyDictionary<string, Rectangle> CalculateBounds(IContainerComponent container, LayoutContext context)
        {
            var result = new Dictionary<string, Rectangle>();
            foreach (var child in container.Children)
            {
                // Просто переносим текущие координаты с учётом коэффициента DPI
                var current = child.BoundingBox;
                result[child.Id] = new Rectangle(
                    (int)(current.X * context.DpiScale),
                    (int)(current.Y * context.DpiScale),
                    (int)(current.Width * context.DpiScale),
                    (int)(current.Height * context.DpiScale)
                );
            }
            return result;
        }
    }
}