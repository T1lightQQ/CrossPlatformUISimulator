using System;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    // Часть 28: Контракт Наблюдателя
    public interface IUIStateObserver : IDisposable
    {
        void OnStateChange(IUIComponent component, UIStateChangeData data);
        void OnError(IUIComponent component, Exception error);
    }

    // Часть 28: Контракт Субъекта нотификации
    public interface IUIStateSubject
    {
        void Attach(IUIStateObserver observer);
        void Detach(IUIStateObserver observer);
        void Notify(UIStateChangeData data);
    }

    // Часть 29: Контракт полиморфного состояния
    public interface IComponentState
    {
        string StateName { get; }
        void Enter(IUIComponent context);
        void Exit(IUIComponent context);
        void HandleClick(IUIComponent context);
        void HandleRender(IUIComponent context, IRenderingContext ctx);
    }
}