using System;
using System.Collections;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    // Абстрактные фабрики (Abstract Factory)
    public interface IThemeFactory
    {
        IRenderingStrategy CreateRenderingStrategy();
    }

    public interface IWidgetFactory
    {
        IUIComponent CreateButton(string id, Rectangle bounds, IUIStyleFlyweight flyweight);
        IUIComponent CreateLabel(string id, Rectangle bounds, IUIStyleFlyweight flyweight);
    }

    // Паттерн Iterator
    public interface IUIComponentIterator : IEnumerator
    {
    }

    public interface IIteratorFactory
    {
        IUIComponentIterator CreateDfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null);
        IUIComponentIterator CreateBfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null);
        IUIComponentIterator CreateProxyAware(IUIComponent root);
    }
}