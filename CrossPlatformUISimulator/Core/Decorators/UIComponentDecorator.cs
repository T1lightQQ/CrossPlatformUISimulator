using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Core.Decorators
{
    public abstract class UIComponentDecorator : IUIComponent, IOriginator
    {
        protected IUIComponent Component;

        protected UIComponentDecorator(IUIComponent component) => Component = component ?? throw new ArgumentNullException(nameof(component));

        public string Id => Component.Id;
        public Rectangle BoundingBox { get => Component.BoundingBox; set => Component.BoundingBox = value; }
        public string TextContent { get => Component.TextContent; set => Component.TextContent = value; }
        public bool Enabled { get => Component.Enabled; set => Component.Enabled = value; }
        public IUIStyleFlyweight Flyweight { get => Component.Flyweight; set => Component.Flyweight = value; }

        public virtual void Render(IRenderingContext ctx) => Component.Render(ctx);
        public void SetPosition(Point position) => Component.SetPosition(position);
        public void SetMediator(IUIComponentMediator mediator) => Component.SetMediator(mediator);
        public T? FindById<T>(string id) where T : class, IUIComponent => Component.FindById<T>(id);

        public IUIComponent GetWrappedComponent() => Component;

        public abstract IUIComponent Clone();

        // Прозрачное делегирование Memento обернутому компоненту
        public IMemento CreateMemento() => ((IOriginator)Component).CreateMemento();
        public void Restore(IMemento memento) => ((IOriginator)Component).Restore(memento);

        public virtual void Dispose() => Component.Dispose();
    }
}