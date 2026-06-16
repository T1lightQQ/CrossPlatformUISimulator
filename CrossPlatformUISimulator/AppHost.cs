using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace CrossPlatformUISimulator
{
    public class AppHost
    {
        private readonly IThemeFactory _themeFactory;
        private readonly IWidgetFactory _widgetFactory;

        public AppHost(IThemeFactory themeFactory, IWidgetFactory widgetFactory)
        {
            _themeFactory = themeFactory;
            _widgetFactory = widgetFactory;
        }

        public void Run(WidgetConfig widgetConfig)
        {
            if (widgetConfig.Theme != _themeFactory.ThemeName)
            {
                throw new InvalidOperationException(
                    $"Ошибка темы! Виджет ожидает тему '{widgetConfig.Theme}', но AppHost запущен с темой '{_themeFactory.ThemeName}'.");
            }

            Console.WriteLine($"\n[Системный лог] Запуск приложения с темой: {_themeFactory.ThemeName}");

            DialogBuilder builder = new DialogBuilder();
            builder.SetTitle("Главное окно")
                   .ConfigureTheme(_themeFactory)
                   .AddButton(new BtnConfig { Text = "Действие" });

            IWidget extraWidget = _widgetFactory.CreateWidget(widgetConfig);
            builder.AddCustomWidget(extraWidget);

            IDialog dialog = builder.Build();
            dialog.ShowDialog();
        }
    }
}