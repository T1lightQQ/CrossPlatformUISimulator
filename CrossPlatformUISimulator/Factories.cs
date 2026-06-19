using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
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
        public IUIComponent CreateWidget(WidgetConfig config, IRenderingStrategy strategy)
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("FactoryMethod", "CreateWidget", TimeSpan.Zero, config.Type.ToString());
            return config.Type switch
            {
                WidgetType.Button => new ButtonComponent(config.Id, config.Bounds, "Кнопка", strategy),
                WidgetType.Label => new LabelComponent(config.Id, config.Bounds, "Текст", strategy),
                WidgetType.Slider => new SliderComponent(config.Id, config.Bounds, 50, strategy),
                WidgetType.Panel => new PanelComponent(config.Id, config.Bounds, strategy),
                _ => throw new NotSupportedException()
            };
        }
    }
}