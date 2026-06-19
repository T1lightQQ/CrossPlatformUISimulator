using System;
using System.Collections.Concurrent;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Infrastructure
{
    
    public class UIStyleFlyweightImpl : IUIStyleFlyweight
    {
        public StyleKey Key { get; }

        public FontMetrics Font => new FontMetrics(Key.FontName, Key.FontSize);

        public ColorPalette Palette => new ColorPalette(
            new Color(Key.R, Key.G, Key.B),
            new Color(0, 0, 0),
            new Color(255, 255, 255)
        );

        public UIStyleFlyweightImpl(StyleKey key) => Key = key;
    }

    
    public class FlyweightFactory
    {
        public static FlyweightFactory Instance { get; } = new FlyweightFactory();

        private readonly ConcurrentDictionary<StyleKey, IUIStyleFlyweight> _cache = new();

        private FlyweightFactory() { }

        public IUIStyleFlyweight GetFlyweight(StyleKey key)
        {
            return _cache.GetOrAdd(key, k => new UIStyleFlyweightImpl(k));
        }
    }
}