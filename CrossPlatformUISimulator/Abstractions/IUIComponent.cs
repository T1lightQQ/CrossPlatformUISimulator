using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    // Паттерн Prototype
    public interface IPrototypical<T> where T : class
    {
        T Clone();
    }

    // Корневой компонент (Базовый интерфейс для Composite, Proxy, Decorator)
    public interface IUIComponent : IPrototypical<IUIComponent>, IDisposable
    {
        string Id { get; }
        Rectangle BoundingBox { get; set; }
        string TextContent { get; set; }
        bool Enabled { get; set; }
        IUIStyleFlyweight Flyweight { get; set; }

        void Render(IRenderingContext ctx);
        void SetPosition(Point position);
        void SetMediator(IUIComponentMediator mediator);
        T? FindById<T>(string id) where T : class, IUIComponent;
    }

    // Интерфейс контейнера для паттерна Composite
    public interface IContainerComponent : IUIComponent
    {
        IReadOnlyList<IUIComponent> Children { get; }
        void AddChild(IUIComponent child);
        void RemoveChild(IUIComponent child);
        void ReplaceChild(string id, IUIComponent newChild);
    }
}