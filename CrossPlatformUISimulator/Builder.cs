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
        public IContainerBuilder AddButton(BtnConfig config) { _buttonsToCreate.Add(config); return this; }
        public IContainerBuilder ConfigureTheme(IThemeFactory theme) { _theme = theme; return this; }
        public IContainerBuilder AddComponent(IUIComponent component) { _customComponents.Add(component); return this; }
        public IContainerBuilder ApplyDecorator(Func<IUIComponent, UIComponentDecorator> decoratorFactory) { _decoratorsToApply.Add(decoratorFactory); return this; }

        public IContainerComponent Build()
        {
            if (_id == null || _bounds == null || _theme == null)
                throw new InvalidOperationException("Недостаточно данных для сборщика.");

            var strategy = _theme.CreateRenderingStrategy();
            var styleFlyweight = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 12, 255, 255, 255));

            var rootPanel = new PanelComponent(_id, _bounds, strategy, styleFlyweight);

            foreach (var btnCfg in _buttonsToCreate)
            {
                var btnStyle = FlyweightFactory.Instance.GetFlyweight(btnCfg.Style);
                rootPanel.AddChild(new ButtonComponent(btnCfg.Id, btnCfg.Bounds, btnCfg.Text, strategy, btnStyle));
            }

            foreach (var comp in _customComponents)
            {
                rootPanel.AddChild(comp);
            }

            IUIComponent finalComponent = rootPanel;
            foreach (var decFactory in _decoratorsToApply)
            {
                finalComponent = decFactory(finalComponent);
            }

            return (IContainerComponent)finalComponent;
        }
    }
}