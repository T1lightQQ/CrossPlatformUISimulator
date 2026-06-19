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
        private readonly ReaderWriterLockSlim _lock = new();

        private IContainerComponent? _rootTree;
        private IThemeFactory _currentFactory;

        public UISystemFacade(
            IThemeFactory initialTheme,
            IWidgetFactory widgetFactory,
            IContainerBuilder builder,
            IApplicationTelemetry telemetry)
        {
            _currentFactory = initialTheme;
            _widgetFactory = widgetFactory;
            _builder = builder;
            _telemetry = telemetry;
        }

        public IContainerComponent CreateDialog(DialogPreset preset, ThemeType theme)
        {
            _lock.EnterWriteLock();
            try
            {
                _currentFactory = theme switch
                {
                    ThemeType.Fluent => new FluentThemeFactory(),
                    ThemeType.Cupertino => new CupertinoThemeFactory(),
                    _ => _currentFactory
                };

                _builder.SetId("FacadeDialogRoot")
                        .SetBounds(preset.Bounds)
                        .ConfigureTheme(_currentFactory)
                        .AddButton(new BtnConfig { Id = "btnOk", Text = preset.Title, Bounds = new Rectangle(10, 10, 150, 35), Style = preset.Style });

                if (preset.Decorators != null)
                {
                    foreach (var decType in preset.Decorators)
                    {
                        switch (decType)
                        {
                            case DecoratorType.Border: _builder.ApplyDecorator(c => new BorderDecorator(c)); break;
                            case DecoratorType.RenderLog: _builder.ApplyDecorator(c => new RenderLogDecorator(c)); break;
                            case DecoratorType.Cached: _builder.ApplyDecorator(c => new CachedRenderDecorator(c)); break;
                        }
                    }
                }

                _rootTree = _builder.Build();
                return _rootTree;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ApplyGlobalTheme(ThemeType theme)
        {
            _lock.EnterWriteLock();
            try
            {
                _currentFactory = theme switch
                {
                    ThemeType.Fluent => new FluentThemeFactory(),
                    ThemeType.Cupertino => new CupertinoThemeFactory(),
                    _ => throw new ArgumentOutOfRangeException(nameof(theme))
                };

                if (_rootTree == null) return;
                IRenderingStrategy newStrategy = _currentFactory.CreateRenderingStrategy();

                var stack = new Stack<IUIComponent>();
                stack.Push(_rootTree);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (current is VirtualComponentProxy proxy)
                    {
                        // Обновляем стратегию прокси без принудительной полной материализации
                        proxy.UpdateStrategy(newStrategy);
                    }
                    else if (current is UIComponentBase baseComponent)
                    {
                        baseComponent.SwitchRenderingStrategy(newStrategy);
                    }
                    else if (current is UIComponentDecorator decorator)
                    {
                        stack.Push(decorator.GetWrappedComponent());
                    }

                    if (current is IContainerComponent container)
                    {
                        foreach (var child in container.Children) stack.Push(child);
                    }
                }

                _telemetry.LogOperation("Facade", "ApplyGlobalTheme", TimeSpan.Zero, theme.ToString());
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RenderAllToContext(IRenderingContext ctx)
        {
            _lock.EnterReadLock();
            try
            {
                _rootTree?.Render(ctx);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void LogCurrentMetrics()
        {
            _lock.EnterReadLock();
            try
            {
                Console.WriteLine($"[Flyweight Статистика]: Hits={FlyweightFactory.Instance.Hits}, Misses={FlyweightFactory.Instance.Misses}, Всего={FlyweightFactory.Instance.TotalInstancesCreated}");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}