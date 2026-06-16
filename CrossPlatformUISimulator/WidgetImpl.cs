using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class StandBtn : IWidget { public void Show() => Console.WriteLine("Рендер: Стандартная кнопка"); }
    public class StandTxt : IWidget { public void Show() => Console.WriteLine("Рендер: Стандартное текстовое поле"); }
    public class StandSlider : IWidget { public void Show() => Console.WriteLine("Рендер: Стандартный слайдер"); }

    public class DbBtn : IWidget { public void Show() => Console.WriteLine("Рендер: [Отладка] Кнопка + Рамка (1px)"); }
    public class DbTxt : IWidget { public void Show() => Console.WriteLine("Рендер: [Отладка] Текстовое поле + Лог системы"); }
    public class DbSlider : IWidget { public void Show() => Console.WriteLine("Рендер: [Отладка] Слайдер + Таймер"); }

    public class StandFactory : IWidgetFactory
    {
        private readonly Dictionary<WidgetType, Func<IWidget>> _registry = new();

        public StandFactory()
        {
            _registry[WidgetType.Btn] = () => new StandBtn();
            _registry[WidgetType.Txt] = () => new StandTxt();
        }

        public void Register(WidgetType type, Func<IWidget> builder)
        {
            _registry[type] = builder;
        }

        public IWidget CreateWidget(WidgetConfig config)
        {
            if (!_registry.TryGetValue(config.Type, out var builder))
            {
                throw new NotSupportedException($"Тип виджета {config.Type} не поддерживается.");
            }
            return builder();
        }
    }

    public class DbFactory : IWidgetFactory
    {
        private readonly Dictionary<WidgetType, Func<IWidget>> _registry = new();

        public DbFactory()
        {
            _registry[WidgetType.Btn] = () => new DbBtn();
            _registry[WidgetType.Txt] = () => new DbTxt();
        }

        public void Register(WidgetType type, Func<IWidget> builder)
        {
            _registry[type] = builder;
        }

        public IWidget CreateWidget(WidgetConfig config)
        {
            if (!_registry.TryGetValue(config.Type, out var builder))
            {
                throw new NotSupportedException($"Тип виджета {config.Type} не поддерживается.");
            }
            return builder();
        }
    }
}