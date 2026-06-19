using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    public interface IPrototypical<T> where T : class
    {
        T Clone();
    }

    public interface IUIComponent : IPrototypical<IUIComponent>, IDisposable, IUIStateSubject
    {
        string Id { get; }
        Rectangle BoundingBox { get; set; }
        string TextContent { get; set; }
        bool Enabled { get; set; }
        IUIStyleFlyweight Flyweight { get; set; }
        IComponentState CurrentState { get; }

        void TransitionTo(IComponentState newState);
        void Render(IRenderingContext ctx);
        void SetPosition(Point position);
        void SetMediator(IUIComponentMediator mediator);
        T? FindById<T>(string id) where T : class, IUIComponent;
    }

    public interface IContainerComponent : IUIComponent
    {
        IReadOnlyList<IUIComponent> Children { get; }
        void AddChild(IUIComponent child);
        void RemoveChild(IUIComponent child);
        void ReplaceChild(string id, IUIComponent newChild);

        // ЧАСТЬ 31: Управление стратегией размещения элементов внутри контейнера
        void SetLayoutStrategy(ILayoutStrategy strategy);
        IReadOnlyDictionary<string, Rectangle> CalculateLayout(LayoutContext context);
    }
}