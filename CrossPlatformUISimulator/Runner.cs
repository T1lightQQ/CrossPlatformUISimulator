using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class EndToEndScenarioRunner
    {
        private readonly IThemeFactory _themeFactory;
        private readonly IWidgetFactory _widgetFactory;
        private readonly IContainerBuilder _builder;
        private readonly IApplicationTelemetry _telemetry;

        public EndToEndScenarioRunner(
            IThemeFactory themeFactory,
            IWidgetFactory widgetFactory,
            IContainerBuilder builder,
            IApplicationTelemetry telemetry)
        {
            _themeFactory = themeFactory;
            _widgetFactory = widgetFactory;
            _builder = builder;
            _telemetry = telemetry;
        }

        public void RunScenario()
        {
            _telemetry.ResetForTesting();
            Console.WriteLine("=== Запуск Сквозного Сценария (6 паттернов) ===");

            MultiChoiceDialogDirector director = new MultiChoiceDialogDirector();
            director.Construct(_builder, _themeFactory);

            IDialog originalDialog = _builder.Build();

            WidgetConfig legacyConfig = new WidgetConfig { Type = WidgetType.LegacyRenderer, Theme = "Fluent" };
            IWidget adaptedWidget = _widgetFactory.CreateWidget(legacyConfig);

            IDialog clonedTemplate = originalDialog.Clone();
            IDialog modifiedDialog = clonedTemplate.AddWidget(adaptedWidget);

            Console.WriteLine("\n=== Отрисовка модифицированного клона с Адаптером ===");
            modifiedDialog.ShowDialog();

            Console.WriteLine("\n=== Сводная статистика телеметрии (Singleton) ===");
            IReadOnlyDictionary<string, int> counts = _telemetry.GetOperationCounts();
            foreach (var kvp in counts)
            {
                Console.WriteLine($"Операция [{kvp.Key}]: Вызвана {kvp.Value} раз(а)");
            }
        }
    }

    public class MultiChoiceDialogDirector
    {
        public void Construct(IContainerBuilder builder, IThemeFactory theme)
        {
            builder.SetTitle("Запрос подтверждения операции")
                   .AddButton(new BtnConfig { Text = "Принять" })
                   .AddButton(new BtnConfig { Text = "Отклонить" })
                   .ConfigureTheme(theme);
        }
    }
}