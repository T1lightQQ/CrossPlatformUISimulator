using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    
    public record Subscription(Predicate<UIEvent> Filter, Action<UIEvent> Handler);

    
    public interface IUIComponentMediator
    {
        void Register(IUIComponent component);
        void Unregister(IUIComponent component);
        void Notify(IUIComponent sender, UIEvent @event);
    }

    
    public interface IEventRouter
    {
        void Route(UIEvent @event, IEnumerable<Subscription> subscriptions);
    }

    
    public interface IValidationHandler
    {
        IValidationHandler SetNext(IValidationHandler handler);
        bool Validate(UIEvent @event);
    }
}