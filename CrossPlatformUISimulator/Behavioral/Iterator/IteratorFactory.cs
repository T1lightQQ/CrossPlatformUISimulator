using System;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Core.Proxies;

namespace CrossPlatformUISimulator.Behavioral.Iterator
{
    public class IteratorFactory : IIteratorFactory
    {
        public IUIComponentIterator CreateDfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null) =>
            new DepthFirstIterator(root, predicate);

        public IUIComponentIterator CreateBfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null) =>
            new BreadthFirstIterator(root);

        // Фабричный метод итератора, который видит "Прокси" и не триггерит их тяжелую загрузку
        public IUIComponentIterator CreateProxyAware(IUIComponent root) =>
            new DepthFirstIterator(root, c => !(c is ILazyComponentProxy p && !p.IsMaterialized));
    }
}