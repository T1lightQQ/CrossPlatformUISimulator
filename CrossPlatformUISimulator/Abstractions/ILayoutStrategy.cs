using System.Collections.Generic;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    
    public interface ILayoutStrategy
    {
        IReadOnlyDictionary<string, Rectangle> CalculateBounds(IContainerComponent container, LayoutContext context);
    }
}