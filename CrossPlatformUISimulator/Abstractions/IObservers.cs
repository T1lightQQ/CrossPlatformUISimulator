using System;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    
    public interface IUIStateObserver : IDisposable
    {
        void OnStateChange(IUIComponent component, UIStateChangeData data);
        void OnError(IUIComponent component, Exception error);
    }

    
    public interface IUIStateSubject
    {
        void Attach(IUIStateObserver observer);
        void Detach(IUIStateObserver observer);
        void Notify(UIStateChangeData data);
    }

    
    public interface IComponentState
    {
        string StateName { get; }
        void Enter(IUIComponent context);
        void Exit(IUIComponent context);
        void HandleClick(IUIComponent context);
        void HandleRender(IUIComponent context, IRenderingContext ctx);
    }
}