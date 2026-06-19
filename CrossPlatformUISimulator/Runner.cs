using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class ApplicationEntryPoint
    {
        public static void RunFullPipeline()
        {
            IThemeFactory theme = new FluentThemeFactory();
            IWidgetFactory widgetFactory = new StandFactory();
            IContainerBuilder builder = new UIContainerBuilder();
            IApplicationTelemetry telemetry = ApplicationTelemetrySingleton.Instance;

            IUISystemFacade facade = new UISystemFacade(theme, widgetFactory, builder, telemetry);

            Console.WriteLine("=== СКВОЗНОЙ СЦЕНАРИЙ (10 ПАТТЕРНОВ) ===");

            DialogPreset preset = new DialogPreset
            {
                Title = "Подтверждение Фасада",
                Bounds = new Rectangle(0, 0, 400, 300),
                Decorators = new[] { DecoratorType.Border, DecoratorType.RenderLog }
            };

            IContainerComponent dialog = facade.CreateDialog(preset, ThemeType.Fluent);

            Console.WriteLine("\nПервичный рендеринг через Фасад:");
            facade.RenderAllToContext(new DefaultRenderingContext());

            Console.WriteLine("\n[Тест Прототипа] Клонирование декорированного поддерева:");
            IContainerComponent clonedDialog = (IContainerComponent)dialog.Clone();

            Console.WriteLine("\n[Тест Моста] Динамическая смена темы:");
            facade.ApplyGlobalTheme(ThemeType.Cupertino);
            facade.RenderAllToContext(new DefaultRenderingContext());

            facade.LogCurrentMetrics();

            RunDecoratorBenchmark();
        }

        private static void RunDecoratorBenchmark()
        {
            Console.WriteLine("\n=== БЕНЧМАРК НАКЛАДНЫХ РАСХОДОВ ДЕКОРАТОРОВ ===");
            IRenderingStrategy strategy = new RasterRenderingStrategy();

            IUIComponent clean = new ButtonComponent("btn", new Rectangle(0, 0, 10, 10), "Text", strategy);
            IUIComponent oneDecorator = new BorderDecorator(clean);
            IUIComponent threeDecorators = new CachedRenderDecorator(new RenderLogDecorator(new BorderDecorator(clean)));

            IRenderingContext dummyContext = new DefaultRenderingContext();

            // Подавляем консольный вывод декораторов для чистоты замера времени
            var originalOut = Console.Out;
            Console.SetOut(System.IO.TextWriter.Null);

            // а) Чистый компонент
            Stopwatch swClean = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++) clean.Render(dummyContext);
            swClean.Stop();

            // б) 1 декоратор
            Stopwatch swOne = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++) oneDecorator.Render(dummyContext);
            swOne.Stop();

            // в) 3 декоратора в цепочке
            Stopwatch swThree = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++) threeDecorators.Render(dummyContext);
            swThree.Stop();

            // Тест Кэширования (Повторный рендеринг кэшированного компонента)
            Stopwatch swCached = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++) threeDecorators.Render(dummyContext);
            swCached.Stop();

            Console.SetOut(originalOut);

            Console.WriteLine($"Чистый компонент (1000 вызовов) : {swClean.Elapsed.TotalMilliseconds:F3} мс");
            Console.WriteLine($"1 Декоратор (1000 вызовов)     : {swOne.Elapsed.TotalMilliseconds:F3} мс");
            Console.WriteLine($"3 Декоратора (1000 вызовов)    : {swThree.Elapsed.TotalMilliseconds:F3} мс");
            Console.WriteLine($"Повторный Кэшированный рендер   : {swCached.Elapsed.TotalMilliseconds:F3} мс");

            double reduction = (1.0 - (swCached.Elapsed.TotalMilliseconds / swThree.Elapsed.TotalMilliseconds)) * 100;
            Console.WriteLine($"Эффективность CachedRenderDecorator: Время сокращено на {reduction:F1}% (Ожидается: >=70%)");
        }
    }
}