using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            IBtn button = _themeFactory.CreateButton();
            ICheck checkBox = _themeFactory.CreateCheckBox();
            IDlg dialog = _themeFactory.CreateDialogRenderer();
            IFont font = _themeFactory.CreateFontEngine();

            IWidget extraWidget = _widgetFactory.CreateWidget(widgetConfig);

            dialog.Open(button, checkBox, extraWidget, font);
        }
    }
}