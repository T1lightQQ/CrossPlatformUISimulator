using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class UIContainerBuilder : IContainerBuilder
    {
        private string? _id;
        private Rectangle? _bounds;
        private IThemeFactory? _theme;
        private readonly List<BtnConfig> _buttonsToCreate = new();
        private readonly List<IUIComponent> _customComponents = new();
        private readonly List<Func<IUIComponent, UIComponentDecorator>> _decoratorsToApply = new();

        public IContainerBuilder SetId(string id) { _id = id; return this; }
        public IContainerBuilder SetBounds(Rectangle bounds) { _bounds = bounds; return this; }

        public IContainerBuilder AddButton(BtnConfig config)
        {
            _buttonsToCreate.Add(config);
            return this;
        }

        public IContainerBuilder ConfigureTheme(IThemeFactory theme) { _theme = theme; return this; }

        public IContainerBuilder AddComponent(IUIComponent component)
        {
            _customComponents.Add(component);
            return this;
        }

        public IContainerBuilder ApplyDecorator(Func<IUIComponent, UIComponentDecorator> decoratorFactory)
        {
            _decoratorsToApply.Add(decoratorFactory);
            return this;
        }

        public IContainerComponent Build()
        {
            if (_id == null || _bounds == null || _theme == null)
                throw new InvalidOperationException("Ошибка валидации данных сборщика.");

            IRenderingStrategy strategy = _theme.CreateRenderingStrategy();
            StyleKey defaultKey = new StyleKey("Arial", 12, 255, 255, 255);
            var styleFlyweight = FlyweightFactory.Instance.GetFlyweight(defaultKey);

            var rootPanel = new PanelComponent(_id, _bounds, strategy, styleFlyweight);

            foreach (var btnCfg in _buttonsToCreate)
            {
                var btnStyle = FlyweightFactory.Instance.GetFlyweight(btnCfg.Style.FontName == null ? defaultKey : btnCfg.Style);
                rootPanel.AddChild(new ButtonComponent(btnCfg.Id, btnCfg.Bounds, btnCfg.Text, strategy, btnStyle));
            }

            foreach (var comp in _customComponents)
            {
                rootPanel.AddChild(comp);
            }

            ValidateTree(rootPanel);

            IUIComponent finalComponent = rootPanel;
            foreach (var decoratorFactory in _decoratorsToApply)
            {
                finalComponent = decoratorFactory(finalComponent);
            }

            return (IContainerComponent)finalComponent;
        }

        private void ValidateTree(IContainerComponent root)
        {
            var visitedContainers = new HashSet<string>();
            var registeredIds = new HashSet<string>();
            var stack = new Stack<IUIComponent>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!registeredIds.Add(current.Id))
                    throw new InvalidOperationException($"Дублирование Id '{current.Id}'!");

                if (current is IContainerComponent container)
                {
                    if (!visitedContainers.Add(container.Id))
                        throw new InvalidOperationException("Обнаружена циклическая ссылка!");

                    foreach (var child in container.Children) stack.Push(child);
                }
            }
        }
    }
}