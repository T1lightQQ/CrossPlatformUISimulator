using System;
using System.Collections;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    
    public interface IThemeFactory
    {
        IRenderingStrategy CreateRenderingStrategy();
    }


    // Абстрактная фабрика
    public interface IWidgetFactory
    {
        IUIComponent CreateButton(string id, Rectangle bounds, IUIStyleFlyweight flyweight);
        IUIComponent CreateLabel(string id, Rectangle bounds, IUIStyleFlyweight flyweight);
    }

    
    public interface IUIComponentIterator : IEnumerator
    {
    }

    // Фабричный метод
    public interface IIteratorFactory
    {
        IUIComponentIterator CreateDfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null);
        IUIComponentIterator CreateBfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null);
        IUIComponentIterator CreateProxyAware(IUIComponent root);
    }
}