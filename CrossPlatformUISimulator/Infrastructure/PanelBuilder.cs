using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Core.Components;
using CrossPlatformUISimulator.Behavioral.Strategy;

namespace CrossPlatformUISimulator.Infrastructure
{
    // Билдер

    // Cтроитель
    public class PanelBuilder
    {
        private readonly string _id;
        private readonly Rectangle _bounds;
        private readonly IUIStyleFlyweight _flyweight;

        private ILayoutStrategy _layoutStrategy = new FreeFormLayoutStrategy();
        private readonly List<IUIComponent> _children = new();

        public PanelBuilder(string id, Rectangle bounds, IUIStyleFlyweight flyweight)
        {
            _id = id;
            _bounds = bounds;
            _flyweight = flyweight;
        }

        // задаем стратегию разметки
        public PanelBuilder WithLayout(ILayoutStrategy strategy)
        {
            _layoutStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            return this;
        }

        // пошагово добавляем кнопку
        public PanelBuilder AddButton(string id, Rectangle bounds)
        {
            _children.Add(new ButtonComponent(id, bounds, _flyweight));
            return this;
        }

        // пошагово добавляем текстовую метку
        public PanelBuilder AddLabel(string id, Rectangle bounds, string text)
        {
            _children.Add(new LabelComponent(id, bounds, _flyweight) { TextContent = text });
            return this;
        }

        // Финальный метод
        public PanelComponent Build()
        {
            var panel = new PanelComponent(_id, _bounds, _flyweight);
            panel.SetLayoutStrategy(_layoutStrategy);

            foreach (var child in _children)
            {
                panel.AddChild(child);
            }

            return panel;
        }
    }
}