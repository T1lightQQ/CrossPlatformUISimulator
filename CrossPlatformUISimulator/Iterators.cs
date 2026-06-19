using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    // Явная реализация конечного автомата обхода дерева без yield return (DFS)
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
            Reset();
        }

        public object Current
        {
            get
            {
                if (!_isInitialized || _current == null)
                    throw new InvalidOperationException("Итератор находится в невалидном состоянии. Вызовите MoveNext().");
                return _current;
            }
        }

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

                // Стратегия работы с Proxy: Пропускаем неинициализированные виртуальные прокси
                if (node is ILazyComponentProxy proxy && !proxy.IsMaterialized)
                {
                    continue;
                }

                // Стратегия работы с декораторами: Декоратор обходится как самостоятельный узел,
                // но его внутреннее содержимое извлекается в стек для обеспечения прозрачной сквозной итерации
                if (node is UIComponentDecorator decorator)
                {
                    _stack.Push(decorator.GetWrappedComponent());
                }
                else if (node is IContainerComponent container)
                {
                    var children = container.Children;
                    // Пушим в обратном порядке для сохранения канонического DFS-обхода слева направо
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
            _current = null;
            _isInitialized = false;
        }

        public void Dispose()
        {
            _stack.Clear();
            _current = null;
        }
    }

    // Канонический фильтрующий итератор (декоратор над базовым итератором)
    public class FilteredIterator : IUIComponentIterator
    {
        private readonly IUIComponentIterator _baseIterator;
        private readonly Func<IUIComponent, bool> _predicate;

        public FilteredIterator(IUIComponentIterator baseIterator, Func<IUIComponent, bool> predicate)
        {
            _baseIterator = baseIterator ?? throw new ArgumentNullException(nameof(baseIterator));
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public object Current => _baseIterator.Current;

        public bool MoveNext()
        {
            while (_baseIterator.MoveNext())
            {
                if (_predicate((IUIComponent)_baseIterator.Current))
                {
                    return true;
                }
            }
            return false;
        }

        public void Reset() => _baseIterator.Reset();
        public void Dispose() => _baseIterator.Dispose();
    }

    // Итератор в ширину (BFS) для обеспечения полиморфизма фабрики обходов
    public class BreadthFirstIterator : IUIComponentIterator
    {
        private readonly IUIComponent _root;
        private readonly Func<IUIComponent, bool>? _predicate;
        private readonly Queue<IUIComponent> _queue = new();
        private IUIComponent? _current;
        private bool _isInitialized = false;

        public BreadthFirstIterator(IUIComponent root, Func<IUIComponent, bool>? predicate = null)
        {
            _root = root;
            _predicate = predicate;
        }

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

                if (node is ILazyComponentProxy proxy && !proxy.IsMaterialized) continue;

                if (node is UIComponentDecorator decorator)
                {
                    _queue.Enqueue(decorator.GetWrappedComponent());
                }
                else if (node is IContainerComponent container)
                {
                    foreach (var child in container.Children)
                    {
                        _queue.Enqueue(child);
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

        public void Reset() { _queue.Clear(); _isInitialized = false; _current = null; }
        public void Dispose() { _queue.Clear(); _current = null; }
    }

    public class IteratorFactory : IIteratorFactory
    {
        public IUIComponentIterator CreateDfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null)
            => new DepthFirstIterator(root, predicate);

        public IUIComponentIterator CreateBfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null)
            => new BreadthFirstIterator(root, predicate);

        public IUIComponentIterator CreateProxyAware(IUIComponent root)
            => new DepthFirstIterator(root, c => !(c is ILazyComponentProxy proxy && !proxy.IsMaterialized));
    }
}