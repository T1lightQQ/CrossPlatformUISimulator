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
        public void LogOperation(string c, string a, TimeSpan d, string? m = null) { }
    }

    public class FlyweightFactory
    {
        public static FlyweightFactory Instance { get; } = new FlyweightFactory();
        private readonly ConcurrentDictionary<StyleKey, IUIStyleFlyweight> _cache = new();

        public IUIStyleFlyweight GetFlyweight(StyleKey k) =>
            _cache.GetOrAdd(k, key => new UIStyleFlyweightImpl
            {
                Font = new FontMetrics(key.FontName, key.FontSize),
                Palette = new ColorPalette(new Color(key.R, key.G, key.B), new Color(0, 0, 0), new Color(255, 255, 255))
            });
    }

    public class UIStyleFlyweightImpl : IUIStyleFlyweight
    {
        public required FontMetrics Font { get; init; }
        public required ColorPalette Palette { get; init; }
    }

    public class DummyRasterStrategy : IRenderingStrategy
    {
        public string StrategyName => "Raster";
        public void DrawBackground(Rectangle r, Color c) { }
    }

    public class UIContainerBuilder : IContainerBuilder
    {
        private string _id = "Root";
        private Rectangle _bounds = new Rectangle(0, 0, 800, 600);

        public IContainerBuilder SetId(string id) { _id = id; return this; }
        public IContainerBuilder SetBounds(Rectangle bounds) { _bounds = bounds; return this; }
        public IContainerBuilder ConfigureTheme(IThemeFactory theme) => this;

        public IContainerComponent Build() => new PanelComponent(_id, _bounds, new DummyRasterStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 12, 0, 0, 0)));
    }
}