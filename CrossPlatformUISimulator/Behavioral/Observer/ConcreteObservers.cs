using System;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Infrastructure;

namespace CrossPlatformUISimulator.Behavioral.Observer
{
    public class TelemetryObserver : IUIStateObserver
    {
        public void OnStateChange(IUIComponent component, UIStateChangeData data)
        {
            TelemetrySingleton.Instance.LogEvent("Dispatched",
                $"[TelemetryObserver] Комп. {component.Id} изменил состояние {data.StateType}: {data.OldValue} -> {data.NewValue}");
        }

        public void OnError(IUIComponent component, Exception error)
        {
            TelemetrySingleton.Instance.LogEvent("Error", $"[TelemetryObserver] Критическая ошибка в {component.Id}: {error.Message}");
        }

        public void Dispose() { }
    }

    public class ValidationObserver : IUIStateObserver
    {
        private readonly IValidationHandler _validationChain;

        public ValidationObserver(IValidationHandler validationChain) => _validationChain = validationChain;

        public void OnStateChange(IUIComponent component, UIStateChangeData data)
        {
            if (data.NewValue == "ErrorState")
            {
                
                var mockEvent = new UIEvent(Guid.NewGuid(), DateTime.UtcNow, "State_Error_Validate", component.Id);
                _validationChain.Validate(mockEvent);
            }
        }

        public void OnError(IUIComponent component, Exception error) { }
        public void Dispose() { }
    }

    public class ThemeSyncObserver : IUIStateObserver
    {
        public void OnStateChange(IUIComponent component, UIStateChangeData data)
        {
            
            if (data.NewValue == "LoadingState")
            {
                component.Flyweight = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 11, 100, 100, 100));
            }
            else if (data.NewValue == "NormalState")
            {
                component.Flyweight = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Segoe UI", 12, 240, 240, 240));
            }
        }

        public void OnError(IUIComponent component, Exception error) { }
        public void Dispose() { }
    }
}