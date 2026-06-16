using System;

namespace CrossPlatformUISimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            IThemeFactory fluentTheme = new FluentThemeFactory();
            StandFactory standardWidgetFactory = new StandFactory();

            standardWidgetFactory.Register(WidgetType.Slider, () => new StandSlider());

            AppHost app1 = new AppHost(fluentTheme, standardWidgetFactory);

            WidgetConfig normalConfig = new WidgetConfig
            {
                Type = WidgetType.Slider,
                Theme = "Fluent"
            };

            app1.Run(normalConfig);

            IThemeFactory cupertinoTheme = new CupertinoThemeFactory();
            DbFactory debugWidgetFactory = new DbFactory();

            AppHost app2 = new AppHost(cupertinoTheme, debugWidgetFactory);

            WidgetConfig badConfig = new WidgetConfig
            {
                Type = WidgetType.Btn,
                Theme = "Fluent"
            };

            try
            {
                app2.Run(badConfig);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"\n[Перехвачено ожидаемое исключение]: {ex.Message}");
            }
        }
    }
}