using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class UISystemFacade : IUISystemFacade
    {
        private readonly IWidgetFactory _widgetFactory;
        private readonly IContainerBuilder _builder;
        private readonly IApplicationTelemetry _telemetry;
        private IContainerComponent? _rootTree;
        private IThemeFactory _currentFactory;

        public IContainerComponent RootTree => _rootTree ?? throw new InvalidOperationException("Дерево UI не инициализировано.");

        public UISystemFacade(IThemeFactory initialTheme, IWidgetFactory widgetFactory, IContainerBuilder builder, IApplicationTelemetry telemetry)
        {
            _currentFactory = initialTheme;
            _widgetFactory = widgetFactory;
            _builder = builder;
            _telemetry = telemetry;
        }

        public IContainerComponent CreateDialog(DialogPreset preset, ThemeType theme)
        {
            _currentFactory = theme == ThemeType.Fluent ? new FluentThemeFactory() : new CupertinoThemeFactory();
            _builder.SetId("RootMainPanel")
                    .SetBounds(preset.Bounds)
                    .ConfigureTheme(_currentFactory);

            _rootTree = _builder.Build();
            return _rootTree;
        }

        public void ApplyGlobalTheme(ThemeType theme) { }
        public void RenderAllToContext(IRenderingContext ctx) => _rootTree?.Render(ctx);
        public void LogCurrentMetrics() { }
    }
}