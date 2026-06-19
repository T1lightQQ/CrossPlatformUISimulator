using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public interface IUIComponent : IDisposable
    {
        string Id { get; }
        Rectangle BoundingBox { get; set; }
        string TextContent { get; set; }
        bool Enabled { get; set; }
        IUIStyleFlyweight Flyweight { get; set; }
        void Render(IRenderingContext ctx);
        void SetPosition(Point position);
        void SetMediator(IUIComponentMediator mediator);
    }

    public interface IContainerComponent : IUIComponent
    {
        System.Collections.Generic.IReadOnlyList<IUIComponent> Children { get; }
        void AddChild(IUIComponent child);
        void RemoveChild(IUIComponent child);
    }

    public interface IUIStyleFlyweight
    {
        FontMetrics Font { get; }
        ColorPalette Palette { get; }
    }

    public interface IUIComponentMediator
    {
        void Register(IUIComponent component);
        void Unregister(IUIComponent component);
        void Notify(IUIComponent sender, UIEvent @event);
    }

    public interface IEventRouter
    {
        void Route(UIEvent @event, System.Collections.Generic.IEnumerable<Subscription> subscriptions);
    }

    public interface IValidationHandler
    {
        IValidationHandler SetNext(IValidationHandler handler);
        bool Validate(UIEvent @event);
    }

    // --- ИНТЕРФЕЙСЫ ПАТТЕРНА MEMENTO (GOF ОПРЕДЕЛЕНИЕ) ---

    // Opaque Token: маркерный интерфейс без публичных методов для защиты инкапсуляции
    public interface IMemento { }

    public interface IOriginator
    {
        IMemento CreateMemento();
        void Restore(IMemento memento);
    }

    public interface IUICommand
    {
        void Execute();
        void Undo();
        string Description { get; }
    }

    public interface IApplicationTelemetry
    {
        void LogEvent(string status, string details);
    }
}