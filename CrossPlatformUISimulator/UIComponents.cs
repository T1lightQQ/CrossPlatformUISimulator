using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public abstract class UIComponentBase : IUIComponent, IOriginator
    {
        protected IUIComponentMediator? Mediator;
        public string Id { get; }
        public Rectangle BoundingBox { get; set; }
        public string TextContent { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public IUIStyleFlyweight Flyweight { get; set; }

        protected UIComponentBase(string id, Rectangle bounds, IUIStyleFlyweight flyweight)
        {
            Id = id;
            BoundingBox = bounds;
            Flyweight = flyweight;
        }

        public void SetMediator(IUIComponentMediator mediator) => Mediator = mediator;

        public abstract void Render(IRenderingContext ctx);
        public void SetPosition(Point pos) => BoundingBox = new Rectangle(pos.X, pos.Y, BoundingBox.Width, BoundingBox.Height);

        // Реализация Originator для атомарного компонента
        public virtual IMemento CreateMemento()
        {
            var states = new Dictionary<string, ExtrinsicComponentState>
            {
                { Id, new ExtrinsicComponentState(BoundingBox, TextContent, Enabled, ((UIStyleFlyweightImpl)Flyweight).Key) }
            };
            return new TreeConfigurationMemento(states);
        }

        public virtual void Restore(IMemento memento)
        {
            if (memento is TreeConfigurationMemento treeMemento)
            {
                if (!treeMemento.SnapshotStates.TryGetValue(Id, out var state))
                    throw new MementoIncompatibleException($"Компонент с ID '{Id}' отсутствует в снапшоте.");

                BoundingBox = state.Bounds;
                TextContent = state.Text;
                Enabled = state.Enabled;
                Flyweight = FlyweightFactory.Instance.GetFlyweight(state.Style);
            }
        }

        public virtual void Dispose() => Mediator?.Unregister(this);
    }

    public class ButtonComponent : UIComponentBase
    {
        public ButtonComponent(string id, Rectangle bounds, IUIStyleFlyweight flyweight) : base(id, bounds, flyweight) { }
        public override void Render(IRenderingContext ctx) { }

        public void SimulateClick()
        {
            Mediator?.Notify(this, new UIEvent(Guid.NewGuid(), DateTime.UtcNow, "UI_Click", Id));
        }
    }

    public class PanelComponent : UIComponentBase, IContainerComponent
    {
        private readonly List<IUIComponent> _children = new();
        public IReadOnlyList<IUIComponent> Children => _children;

        public PanelComponent(string id, Rectangle bounds, IUIStyleFlyweight flyweight) : base(id, bounds, flyweight) { }

        public void AddChild(IUIComponent child) => _children.Add(child);
        public void RemoveChild(IUIComponent child) => _children.Remove(child);

        public override void Render(IRenderingContext ctx)
        {
            foreach (var child in _children) child.Render(ctx);
        }

        // Рекурсивный сбор снапшотов по всему Composite-дереву
        public override IMemento CreateMemento()
        {
            var states = new Dictionary<string, ExtrinsicComponentState>
            {
                { Id, new ExtrinsicComponentState(BoundingBox, TextContent, Enabled, ((UIStyleFlyweightImpl)Flyweight).Key) }
            };

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

        // Рекурсивное восстановление и жесткая структурная валидация
        public override void Restore(IMemento memento)
        {
            if (memento is TreeConfigurationMemento treeMemento)
            {
                if (!treeMemento.SnapshotStates.TryGetValue(Id, out var state))
                    throw new MementoIncompatibleException($"Контейнер '{Id}' удален из структуры дерева конфигурации.");

                BoundingBox = state.Bounds;
                TextContent = state.Text;
                Enabled = state.Enabled;
                Flyweight = FlyweightFactory.Instance.GetFlyweight(state.Style);

                foreach (var child in _children)
                {
                    if (child is IOriginator childOriginator)
                    {
                        childOriginator.Restore(treeMemento);
                    }
                }
            }
        }
    }

    // Декоратор прозрачно делегирует вызовы Memento обернутому компоненту
    public abstract class UIComponentDecorator : IUIComponent, IOriginator
    {
        protected IUIComponent Component;
        protected UIComponentDecorator(IUIComponent component) => Component = component;

        public string Id => Component.Id;
        public Rectangle BoundingBox { get => Component.BoundingBox; set => Component.BoundingBox = value; }
        public string TextContent { get => Component.TextContent; set => Component.TextContent = value; }
        public bool Enabled { get => Component.Enabled; set => Component.Enabled = value; }
        public IUIStyleFlyweight Flyweight { get => Component.Flyweight; set => Component.Flyweight = value; }

        public void Render(IRenderingContext ctx) => Component.Render(ctx);
        public void SetPosition(Point position) => Component.SetPosition(position);
        public void SetMediator(IUIComponentMediator mediator) => Component.SetMediator(mediator);

        public IMemento CreateMemento() => ((IOriginator)Component).CreateMemento();
        public void Restore(IMemento memento) => ((IOriginator)Component).Restore(memento);
        public void Dispose() => Component.Dispose();
    }

    public class BorderDecorator : UIComponentDecorator
    {
        public BorderDecorator(IUIComponent component) : base(component) { }
    }
}