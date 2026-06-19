using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Infrastructure;
using CrossPlatformUISimulator.Behavioral.Memento;

namespace CrossPlatformUISimulator.Core.Components
{
    public class PanelComponent : UIComponentBase, IContainerComponent
    {
        private readonly List<IUIComponent> _children = new();
        public IReadOnlyList<IUIComponent> Children => _children;

        public PanelComponent(string id, Rectangle bounds, IUIStyleFlyweight flyweight)
            : base(id, bounds, flyweight) { }

        public void AddChild(IUIComponent child) => _children.Add(child);
        public void RemoveChild(IUIComponent child) => _children.Remove(child);

        public void ReplaceChild(string id, IUIComponent newChild)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i].Id == id)
                {
                    _children[i] = newChild;
                    return;
                }
            }
        }

        public override void Render(IRenderingContext ctx)
        {
            foreach (var child in _children) child.Render(ctx);
        }

        public override T? FindById<T>(string id) where T : class
        {
            if (Id == id && this is T rootTarget) return rootTarget;

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
            return clone;
        }

        #region Рекурсивный сбор и восстановление Memento по всему дереву Composite

        public override IMemento CreateMemento()
        {
            var styleImpl = (UIStyleFlyweightImpl)Flyweight;
            var states = new Dictionary<string, ExtrinsicComponentState>
            {
                { Id, new ExtrinsicComponentState(BoundingBox, TextContent, Enabled, styleImpl.Key) }
            };

            // Собираем состояния всех дочерних элементов (включая вложенные контейнеры)
            foreach (var child in _children)
            {
                if (child is IOriginator childOriginator)
                {
                    var childMemento = (TreeConfigurationMemento)childOriginator.CreateMemento();
                    foreach (var pair in childMemento.SnapshotStates)
                    {
                        states[pair.Key] = pair.Value;
                    }
                }
            }
            return new TreeConfigurationMemento(states);
        }

        public override void Restore(IMemento memento)
        {
            if (memento is TreeConfigurationMemento treeMemento)
            {
                if (!treeMemento.SnapshotStates.TryGetValue(Id, out var state))
                    throw new MementoIncompatibleException($"Контейнер '{Id}' был удален из топологии дерева.");

                BoundingBox = state.Bounds;
                TextContent = state.Text;
                Enabled = state.Enabled;
                Flyweight = FlyweightFactory.Instance.GetFlyweight(state.Style);

                // Каскадно передаем команду восстановления дочерним узлам
                foreach (var child in _children)
                {
                    if (child is IOriginator childOriginator)
                    {
                        childOriginator.Restore(treeMemento);
                    }
                }
            }
        }

        #endregion
    }
}