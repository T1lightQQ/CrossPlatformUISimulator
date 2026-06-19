using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public abstract class UIEventHandlerBase : IUIEventHandler
    {
        private IUIEventHandler? _nextHandler;

        public IUIEventHandler SetNext(IUIEventHandler next)
        {
            _nextHandler = next;
            return this;
        }

        public virtual bool Handle(UIEvent @event)
        {
            if (_nextHandler != null)
            {
                return _nextHandler.Handle(@event);
            }
            return false;
        }
    }

    // 1. Валидация инвариантов бизнес-логики и границ координат
    public class ValidationHandler : UIEventHandlerBase
    {
        private readonly IUISystemFacade _facade;

        public ValidationHandler(IUISystemFacade facade) => _facade = facade;

        public override bool Handle(UIEvent @event)
        {
            if (@event.EventType == "Move")
            {
                var points = (Point)@event.Payload;
                if (points.X < 0 || points.Y < 0 || points.X > 4000 || points.Y > 4000)
                {
                    ApplicationTelemetrySingleton.Instance.LogOperation("CoR", "ValidationReject", TimeSpan.Zero, "Coordinates out of bounds.");
                    return true; // Прерываем цепочку, событие скомпрометировано
                }

                // Проверка на ProtectionProxy
                var comp = _facade.RootTree.FindById<IProtectionProxy>(@event.TargetComponentId);
                if (comp != null && comp.IsLocked)
                {
                    ApplicationTelemetrySingleton.Instance.LogOperation("CoR", "ValidationReject", TimeSpan.Zero, "Component is locked by ProtectionProxy.");
                    return true;
                }
            }
            return base.Handle(@event);
        }
    }

    // 2. Защита от дребезга и слишком частых событий (Throttling)
    public class ThrottlingHandler : UIEventHandlerBase
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _period;
        private readonly ConcurrentDictionary<string, (long Ticks, int Count)> _metrics = new();

        public ThrottlingHandler(int maxRequests, TimeSpan period)
        {
            _maxRequests = maxRequests;
            _period = period;
        }

        public override bool Handle(UIEvent @event)
        {
            long nowTicks = DateTime.UtcNow.Ticks;
            string key = @event.TargetComponentId;

            var current = _metrics.AddOrUpdate(key,
                (nowTicks, 1),
                (_, old) =>
                {
                    if (nowTicks - old.Ticks < _period.Ticks)
                    {
                        return (old.Ticks, old.Count + 1);
                    }
                    return (nowTicks, 1);
                });

            if (current.Count > _maxRequests && nowTicks - current.Ticks < _period.Ticks)
            {
                ApplicationTelemetrySingleton.Instance.LogOperation("CoR", "Throttled", TimeSpan.Zero, key);
                return true; // Отбрасываем событие
            }

            return base.Handle(@event);
        }
    }

    // 3. Маршрутизация события до конкретного компонента в иерархическом дереве
    public class RoutingHandler : UIEventHandlerBase
    {
        private readonly IUISystemFacade _facade;
        private readonly Action<IUIComponent, UIEvent> _onRoutedCallback;

        public RoutingHandler(IUISystemFacade facade, Action<IUIComponent, UIEvent> onRoutedCallback)
        {
            _facade = facade;
            _onRoutedCallback = onRoutedCallback;
        }

        public override bool Handle(UIEvent @event)
        {
            var component = _facade.RootTree.FindById<IUIComponent>(@event.TargetComponentId);
            if (component != null)
            {
                _onRoutedCallback(component, @event);
                return true; // Успешно обработано и завершено
            }
            return base.Handle(@event);
        }
    }

    // 4. Терминальный обработчик по умолчанию (Гарантия корректного выхода)
    public class FallbackHandler : UIEventHandlerBase
    {
        public override bool Handle(UIEvent @event)
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("CoR", "FallbackUnhandled", TimeSpan.Zero, @event.TargetComponentId);
            return true;
        }
    }
}