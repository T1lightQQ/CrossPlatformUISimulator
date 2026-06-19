using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    // Описание подписки (используется внутри медиатора)
    public record Subscription(Predicate<UIEvent> Filter, Action<UIEvent> Handler);

    // Паттерн Mediator
    public interface IUIComponentMediator
    {
        void Register(IUIComponent component);
        void Unregister(IUIComponent component);
        void Notify(IUIComponent sender, UIEvent @event);
    }

    // Стратегия маршрутизации внутри Посредника
    public interface IEventRouter
    {
        void Route(UIEvent @event, IEnumerable<Subscription> subscriptions);
    }

    // Паттерн Chain of Responsibility (Цепочка обязанностей для валидации событий)
    public interface IValidationHandler
    {
        IValidationHandler SetNext(IValidationHandler handler);
        bool Validate(UIEvent @event);
    }
}