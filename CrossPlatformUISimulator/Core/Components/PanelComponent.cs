using System;
using System.Collections.Generic;
using System.Threading;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Behavioral.Memento;
using CrossPlatformUISimulator.Behavioral.Strategy;
using CrossPlatformUISimulator.Infrastructure;

namespace CrossPlatformUISimulator.Core.Components
{
    public class PanelComponent : UIComponentBase, IContainerComponent
    {
        private readonly List<IUIComponent> _children = new();
        private ILayoutStrategy _layoutStrategy = new FreeFormLayoutStrategy();
        private readonly object _layoutLock = new();

        public IReadOnlyList<IUIComponent> Children => _children;

        public PanelComponent(string id, Rectangle bounds, IUIStyleFlyweight flyweight)
            : base(id, bounds, flyweight) { }

        public void SetLayoutStrategy(ILayoutStrategy strategy)
        {
            lock (_layoutLock)
            {
                _layoutStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            }
        }

        public IReadOnlyDictionary<string, Rectangle> CalculateLayout(LayoutContext context)
        {
            ILayoutStrategy currentStrategy;
            lock (_layoutLock)
            {
                currentStrategy = _layoutStrategy;
            }
            return currentStrategy.CalculateBounds(this, context);
        }

        public void AddChild(IUIComponent child)
        {
            _children.Add(child);
            child.SetMediator(Mediator);
        }

        public void RemoveChild(IUIComponent child)
        {
            _children.Remove(child);
        }

        public void ReplaceChild(string id, IUIComponent newChild)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i].Id == id)
                {
                    _children[i] = newChild;
                    newChild.SetMediator(Mediator);
                    return;
                }
            }
        }

        public override void Render(IRenderingContext ctx)
        {
            foreach (var child in _children)
            {
                child.Render(ctx);
            }
        }

        public override T? FindById<T>(string id) where T : class
        {
            if (Id == id && this is T target) return target;

            foreach (var child in _children)
            {
                var found = child.FindById<T>(id);
                if (found != null) return found;
            }
            return null;
        }

        public override IUIComponent Clone()
        {
            var clone = new PanelComponent(Id, BoundingBox, Flyweight) { Enabled = Enabled, TextContent = TextContent };
            foreach (var child in _children)
            {
                clone.AddChild(child.Clone());
            }
            lock (_layoutLock)
            {
                clone.SetLayoutStrategy(_layoutStrategy);
            }
            return clone;
        }

        public override IMemento CreateMemento()
        {
            var styleImpl = (UIStyleFlyweightImpl)Flyweight;
            var states = new Dictionary<string, ExtrinsicComponentState>
            {
                { Id, new ExtrinsicComponentState(BoundingBox, TextContent, Enabled, styleImpl.Key, CurrentState.StateName) }
            };

            foreach (var child in _children)
            {
                if (child is UIComponentBase childBase)
                {
                    var childMemento = (TreeConfigurationMemento)childBase.CreateMemento();
                    foreach (var kvp in childMemento.SnapshotStates)
                    {
                        states[kvp.Key] = kvp.Value;
                    }
                }
            }
            return new TreeConfigurationMemento(states);
        }

        public override void Restore(IMemento memento)
        {
            base.Restore(memento);
            foreach (var child in _children)
            {
                if (child is UIComponentBase childBase)
                {
                    childBase.Restore(memento);
                }
            }
        }
    }
}