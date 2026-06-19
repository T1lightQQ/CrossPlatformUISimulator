using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Core.Decorators
{
    public abstract class UIComponentDecorator : IUIComponent
    {
        protected readonly IUIComponent WrappedComponent;

        protected UIComponentDecorator(IUIComponent component)
        {
            WrappedComponent = component ?? throw new ArgumentNullException(nameof(component));
        }

        public virtual string Id => WrappedComponent.Id;

        public virtual Rectangle BoundingBox
        {
            get => WrappedComponent.BoundingBox;
            set => WrappedComponent.BoundingBox = value;
        }

        public virtual string TextContent
        {
            get => WrappedComponent.TextContent;
            set => WrappedComponent.TextContent = value;
        }

        public virtual bool Enabled
        {
            get => WrappedComponent.Enabled;
            set => WrappedComponent.Enabled = value;
        }

        public virtual IUIStyleFlyweight Flyweight
        {
            get => WrappedComponent.Flyweight;
            set => WrappedComponent.Flyweight = value;
        }

        
        public virtual IComponentState CurrentState => WrappedComponent.CurrentState;
        public virtual void TransitionTo(IComponentState newState) => WrappedComponent.TransitionTo(newState);

        public virtual void Attach(IUIStateObserver observer) => WrappedComponent.Attach(observer);
        public virtual void Detach(IUIStateObserver observer) => WrappedComponent.Detach(observer);
        public virtual void Notify(UIStateChangeData data) => WrappedComponent.Notify(data);

        public virtual void Render(IRenderingContext ctx) => WrappedComponent.Render(ctx);
        public virtual void SetPosition(Point position) => WrappedComponent.SetPosition(position);
        public virtual void SetMediator(IUIComponentMediator mediator) => WrappedComponent.SetMediator(mediator);

        public virtual T? FindById<T>(string id) where T : class, IUIComponent => WrappedComponent.FindById<T>(id);
        public abstract IUIComponent Clone();

        public IUIComponent GetWrappedComponent() => WrappedComponent;

        
        public virtual void Accept(IUIComponentVisitor visitor)
        {
            visitor.Visit(this);
            WrappedComponent.Accept(visitor);
        }

        public virtual void Dispose() => WrappedComponent.Dispose();
    }
}