using System;

namespace CrossPlatformUISimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            IThemeFactory theme = new FluentThemeFactory();
            IWidgetFactory widgetFactory = new StandFactory();
            IContainerBuilder builder = new DialogBuilder();
            IApplicationTelemetry telemetry = ApplicationTelemetrySingleton.Instance;

            EndToEndScenarioRunner runner = new EndToEndScenarioRunner(theme, widgetFactory, builder, telemetry);
            runner.RunScenario();
        }
    }
}