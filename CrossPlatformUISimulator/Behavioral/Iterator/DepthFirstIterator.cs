using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Core.Decorators; 

namespace CrossPlatformUISimulator.Behavioral.Iterator
{
    public class DepthFirstIterator : IUIComponentIterator
    {
        private readonly IUIComponent _root;
        private readonly Func<IUIComponent, bool>? _predicate;
        private readonly Stack<IUIComponent> _stack = new();
        private IUIComponent? _current;
        private bool _isInitialized = false;

        public DepthFirstIterator(IUIComponent root, Func<IUIComponent, bool>? predicate = null)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _predicate = predicate;
        }

        public object Current => _current ?? throw new InvalidOperationException("Итератор вне диапазона");

        public bool MoveNext()
        {
            if (!_isInitialized)
            {
                _stack.Push(_root);
                _isInitialized = true;
            }

            while (_stack.Count > 0)
            {
                var node = _stack.Pop();

                
                if (node is Core.Proxies.ILazyComponentProxy proxy && !proxy.IsMaterialized)
                    continue;

                
                if (node is UIComponentDecorator decorator)
                {
                    _stack.Push(decorator.GetWrappedComponent());
                }
                else if (node is IContainerComponent container)
                {
                    var children = container.Children;
                    for (int i = children.Count - 1; i >= 0; i--)
                    {
                        _stack.Push(children[i]);
                    }
                }

                if (_predicate == null || _predicate(node))
                {
                    _current = node;
                    return true;
                }
            }
            _current = null;
            return false;
        }

        public void Reset()
        {
            _stack.Clear();
            _isInitialized = false;
            _current = null;
        }

        public void Dispose() => _stack.Clear();
    }
}