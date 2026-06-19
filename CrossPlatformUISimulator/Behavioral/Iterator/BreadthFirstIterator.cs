using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Core.Decorators;

namespace CrossPlatformUISimulator.Behavioral.Iterator
{
    public class BreadthFirstIterator : IUIComponentIterator
    {
        private readonly IUIComponent _root;
        private readonly Queue<IUIComponent> _queue = new();
        private IUIComponent? _current;
        private bool _isInitialized = false;

        public BreadthFirstIterator(IUIComponent root) => _root = root;

        public object Current => _current ?? throw new InvalidOperationException();

        public bool MoveNext()
        {
            if (!_isInitialized)
            {
                _queue.Enqueue(_root);
                _isInitialized = true;
            }

            while (_queue.Count > 0)
            {
                var node = _queue.Dequeue();

                if (node is UIComponentDecorator d)
                    _queue.Enqueue(d.GetWrappedComponent());
                else if (node is IContainerComponent c)
                {
                    foreach (var ch in c.Children) _queue.Enqueue(ch);
                }

                _current = node;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _queue.Clear();
            _isInitialized = false;
        }

        public void Dispose() => _queue.Clear();
    }
}