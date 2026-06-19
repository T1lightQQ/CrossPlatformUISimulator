using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public record Subscription(Predicate<UIEvent> Filter, Action<UIEvent> Handler);

    // Событийно-ориентированный Посредник с конфигурируемой стратегией маршрутизации
    public class EventDrivenMediator : IUIComponentMediator
    {
        private readonly ConcurrentDictionary<string, WeakReference<IUIComponent>> _registry = new();
        private readonly List<Subscription> _subscriptions = new();
        private readonly object _subLock = new();
        private readonly IEventRouter _router;
        private readonly IValidationHandler? _validationChain;

        public EventDrivenMediator(IEventRouter router, IValidationHandler? validationChain = null)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _validationChain = validationChain;
        }

        public void Register(IUIComponent component)
        {
            if (component == null) return;
            _registry[component.Id] = new WeakReference<IUIComponent>(component);
            component.SetMediator(this);
        }

        public void Unregister(IUIComponent component)
        {
            if (component == null) return;
            _registry.TryRemove(component.Id, out _);
        }

        public void AddSubscription(Predicate<UIEvent> filter, Action<UIEvent> handler)
        {
            lock (_subLock)
            {
                _subscriptions.Add(new Subscription(filter, handler));
            }
        }

        public void Notify(IUIComponent sender, UIEvent @event)
        {
            // Верификация события через Chain of Responsibility перед диспетчеризацией
            if (_validationChain != null && !_validationChain.Validate(@event))
            {
                TelemetrySingleton.Instance.LogEvent("Filtered", $"Событие от {sender.Id} отклонено цепочкой валидации.");
                return;
            }

            List<Subscription> activeSubs;
            lock (_subLock)
            {
                activeSubs = new List<Subscription>(_subscriptions);
            }

            // Делегирование стратегии маршрутизации
            _router.Route(@event, activeSubs);
            TelemetrySingleton.Instance.LogEvent("Dispatched", $"Событие {@event.EventType} обработано медиатором.");
        }
    }

    // Конкретная стратегия маршрутизации событий
    public class SequentialEventRouter : IEventRouter
    {
        public void Route(UIEvent @event, IEnumerable<Subscription> subscriptions)
        {
            foreach (var sub in subscriptions)
            {
                if (sub.Filter(@event))
                {
                    sub.Handler(@event);
                }
            }
        }
    }

    // Цепочка обязанностей (Chain of Responsibility) для фильтрации и троттлинга событий
    public abstract class BaseValidationHandler : IValidationHandler
    {
        private IValidationHandler? _next;

        public IValidationHandler SetNext(IValidationHandler handler)
        {
            _next = handler;
            return handler;
        }

        public virtual bool Validate(UIEvent @event)
        {
            if (_next != null) return _next.Validate(@event);
            return true;
        }
    }

    public class SpamProtectionHandler : BaseValidationHandler
    {
        public override bool Validate(UIEvent @event)
        {
            if (@event.EventType == "ClickSpam") return false;
            return base.Validate(@event);
        }
    }
}