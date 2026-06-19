using System.Collections.Generic;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    // Часть 31: Стратегия — инкапсулирует алгоритмы расчёта геометрии UI
    public interface ILayoutStrategy
    {
        IReadOnlyDictionary<string, Rectangle> CalculateBounds(IContainerComponent container, LayoutContext context);
    }
}