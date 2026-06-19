using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Infrastructure;      // Для FlyweightFactory (создадим позже)
using CrossPlatformUISimulator.Behavioral.Memento;  // Для конкретного Memento (создадим позже)

namespace CrossPlatformUISimulator.Core.Components
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

        public void SetPosition(Point pos) =>
            BoundingBox = new Rectangle(pos.X, pos.Y, BoundingBox.Width, BoundingBox.Height);

        // Поиск компонента вниз по дереву (для базового элемента возвращает себя, если ID совпал)
        public virtual T? FindById<T>(string id) where T : class, IUIComponent
        {
            if (Id == id && this is T target) return target;
            return null;
        }

        // Паттерн Prototype
        public abstract IUIComponent Clone();

        #region Реализация паттерна Memento (Originator)

        public virtual IMemento CreateMemento()
        {
            // Извлекаем внутренний неизменяемый ключ стиля (Flyweight), чтобы не копировать весь объект стиля
            var styleImpl = (UIStyleFlyweightImpl)Flyweight;

            var states = new Dictionary<string, ExtrinsicComponentState>
            {
                { Id, new ExtrinsicComponentState(BoundingBox, TextContent, Enabled, styleImpl.Key) }
            };
            return new TreeConfigurationMemento(states);
        }

        public virtual void Restore(IMemento memento)
        {
            if (memento is TreeConfigurationMemento treeMemento)
            {
                if (!treeMemento.SnapshotStates.TryGetValue(Id, out var state))
                    throw new MementoIncompatibleException($"Компонент с ID '{Id}' отсутствует в восстанавливаемом снапшоте.");

                BoundingBox = state.Bounds;
                TextContent = state.Text;
                Enabled = state.Enabled;

                // Восстанавливаем ссылку на Flyweight через фабрику разделяемых ресурсов
                Flyweight = FlyweightFactory.Instance.GetFlyweight(state.Style);
            }
        }

        #endregion

        public virtual void Dispose() => Mediator?.Unregister(this);
    }
}