using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Infrastructure; 

namespace CrossPlatformUISimulator.Behavioral.Mediator
{
    public class EventDrivenMediator : IUIComponentMediator
    {
        private readonly ConcurrentDictionary<string, WeakReference<IUIComponent>> _registry = new();
        private readonly List<Subscription> _subs = new();
        private readonly object _lock = new();
        private readonly IEventRouter _router;
        private readonly IValidationHandler? _validator;

        public EventDrivenMediator(IEventRouter router, IValidationHandler? validator = null)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _validator = validator;
        }

        public void Register(IUIComponent c)
        {
            if (c != null)
            {
                _registry[c.Id] = new WeakReference<IUIComponent>(c);
                c.SetMediator(this);
            }
        }

        public void Unregister(IUIComponent c)
        {
            if (c != null) _registry.TryRemove(c.Id, out _);
        }

        public void AddSub(Predicate<UIEvent> filter, Action<UIEvent> handler)
        {
            lock (_lock)
            {
                _subs.Add(new Subscription(filter, handler));
            }
        }

        public void Notify(IUIComponent sender, UIEvent @event)
        {
            
            if (_validator != null && !_validator.Validate(@event)) return;

            List<Subscription> targets;
            lock (_lock)
            {
                targets = new List<Subscription>(_subs);
            }

            
            _router.Route(@event, targets);

            
            TelemetrySingleton.Instance.LogEvent("Dispatched", $"Event {@event.EventType} from {sender.Id}");
        }
    }
}