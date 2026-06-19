using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public record UIStyleFlyweight : IUIStyleFlyweight
    {
        public Guid StyleId { get; init; } = Guid.NewGuid();
        public required FontMetrics Font { get; init; }
        public required ColorPalette Palette { get; init; }
        public byte[]? IconBytes { get; init; }
    }

    public class FlyweightFactory
    {
        private static readonly Lazy<FlyweightFactory> _instance = new(() => new FlyweightFactory());
        public static FlyweightFactory Instance => _instance.Value;

        private readonly ConcurrentDictionary<StyleKey, IUIStyleFlyweight> _pool = new();

        private int _hits;
        private int _misses;
        private int _totalCreated;

        public int Hits => _hits;
        public int Misses => _misses;
        public int TotalInstancesCreated => _totalCreated;

        public IUIStyleFlyweight GetFlyweight(StyleKey key)
        {
            if (_pool.TryGetValue(key, out var style))
            {
                Interlocked.Increment(ref _hits);
                return style;
            }

            var newStyle = new UIStyleFlyweight
            {
                Font = new FontMetrics(key.FontName, key.FontSize),
                Palette = new ColorPalette(new Color(key.R, key.G, key.B), new Color(0, 0, 0), new Color(255, 255, 255)),
                IconBytes = new byte[1024]
            };

            if (_pool.TryAdd(key, newStyle))
            {
                Interlocked.Increment(ref _misses);
                Interlocked.Increment(ref _totalCreated);
                return newStyle;
            }

            return _pool[key];
        }

        public void ResetMetrics()
        {
            _hits = 0;
            _misses = 0;
            _totalCreated = 0;
        }
    }

    public class FluentThemeFactory : IThemeFactory
    {
        public string ThemeName => "Fluent";
        public IRenderingStrategy CreateRenderingStrategy() => new RasterRenderingStrategy();
    }

    public class CupertinoThemeFactory : IThemeFactory
    {
        public string ThemeName => "Cupertino";
        public IRenderingStrategy CreateRenderingStrategy() => new RasterRenderingStrategy();
    }

    public class StandFactory : IWidgetFactory
    {
        public IUIComponent CreateWidget(WidgetConfig config, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
        {
            var style = flyweight ?? FlyweightFactory.Instance.GetFlyweight(config.Style);
            return config.Type switch
            {
                WidgetType.Button => new ButtonComponent(config.Id, config.Bounds, "Кнопка", strategy, style),
                WidgetType.Label => new LabelComponent(config.Id, config.Bounds, "Текст", strategy, style),
                WidgetType.Slider => new SliderComponent(config.Id, config.Bounds, strategy, style),
                WidgetType.Panel => new PanelComponent(config.Id, config.Bounds, strategy, style),
                _ => throw new NotSupportedException()
            };
        }
    }
}