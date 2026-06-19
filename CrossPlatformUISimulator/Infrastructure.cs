using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class TelemetrySingleton : IApplicationTelemetry
    {
        public static TelemetrySingleton Instance { get; } = new TelemetrySingleton();
        private int _delivered = 0;

        public void LogEvent(string status, string details)
        {
            if (status == "Dispatched") System.Threading.Interlocked.Increment(ref _delivered);
        }

        public int GetDeliveredCount() => _delivered;
    }

    public class FlyweightFactory
    {
        public static FlyweightFactory Instance { get; } = new FlyweightFactory();
        private readonly ConcurrentDictionary<StyleKey, IUIStyleFlyweight> _cache = new();

        public IUIStyleFlyweight GetFlyweight(StyleKey k) =>
            _cache.GetOrAdd(k, key => new UIStyleFlyweightImpl(key));
    }

    public class UIStyleFlyweightImpl : IUIStyleFlyweight
    {
        public StyleKey Key { get; }
        public FontMetrics Font => new FontMetrics(Key.FontName, Key.FontSize);
        public ColorPalette Palette => new ColorPalette(new Color(Key.R, Key.G, Key.B), new Color(0, 0, 0), new Color(0, 0, 0));

        public UIStyleFlyweightImpl(StyleKey key) => Key = key;
    }

    public class UISystemFacade
    {
        private readonly EventDrivenMediator _mediator;
        public PanelComponent Root { get; }

        public UISystemFacade(EventDrivenMediator mediator, PanelComponent root)
        {
            _mediator = mediator;
            Root = root;
        }

        public void Subscribe(Predicate<UIEvent> filter, Action<UIEvent> handler) => _mediator.AddSubscription(filter, handler);
        public void Publish(IUIComponent sender, UIEvent @event) => _mediator.Notify(sender, @event);
    }
}