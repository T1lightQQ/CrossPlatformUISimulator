using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class LegacyGraphicsEngine
    {
        public void InitializeRawContext(IntPtr windowHandle) { }
        public void DrawNativeButton(int x, int y, int width, int height, string legacyLabel) { }
        public void RenderTextRaster(string fontName, int size, int r, int g, int b, int x, int y, string text) { }
        public void ShowModalWindow(IntPtr parent, string title, bool blockInput) { }
    }

    public class LegacyGraphicsAdapter : IWidget
    {
        private readonly LegacyGraphicsEngine _legacyEngine;

        public LegacyGraphicsAdapter()
        {
            _legacyEngine = new LegacyGraphicsEngine();
        }

        public void Show()
        {
            _legacyEngine.InitializeRawContext(IntPtr.Zero);
            _legacyEngine.ShowModalWindow(IntPtr.Zero, "Адаптированное Окно Legacy", true);
        }

        public IWidget Clone()
        {
            var sw = Stopwatch.StartNew();
            var clone = new LegacyGraphicsAdapter();
            sw.Stop();

            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", sw.Elapsed, "LegacyGraphicsAdapter");
            return clone;
        }
    }

    public class StandBtn : IWidget
    {
        public void Show() => Console.WriteLine("Виджет: Стандартная кнопка");
        public IWidget Clone()
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", TimeSpan.Zero, "StandBtn");
            return new StandBtn();
        }
    }

    public class StandTxt : IWidget
    {
        public void Show() => Console.WriteLine("Виджет: Текстовое поле");
        public IWidget Clone()
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", TimeSpan.Zero, "StandTxt");
            return new StandTxt();
        }
    }

    public class StandSlider : IWidget
    {
        public void Show() => Console.WriteLine("Виджет: Стандартный слайдер");
        public IWidget Clone()
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", TimeSpan.Zero, "StandSlider");
            return new StandSlider();
        }
    }

    public class DbBtn : IWidget
    {
        public void Show() => Console.WriteLine("Виджет: [Отладка] Кнопка + Рамка (1px)");
        public IWidget Clone()
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", TimeSpan.Zero, "DbBtn");
            return new DbBtn();
        }
    }

    public class DbTxt : IWidget
    {
        public void Show() => Console.WriteLine("Виджет: [Отладка] Текстовое поле + Лог системы");
        public IWidget Clone()
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", TimeSpan.Zero, "DbTxt");
            return new DbTxt();
        }
    }

    public class DbSlider : IWidget
    {
        public void Show() => Console.WriteLine("Виджет: [Отладка] Слайдер + Таймер");
        public IWidget Clone()
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", TimeSpan.Zero, "DbSlider");
            return new DbSlider();
        }
    }
}