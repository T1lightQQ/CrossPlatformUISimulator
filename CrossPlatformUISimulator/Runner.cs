using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class ScaleTestRunner
    {
        public static void RunScaleTests()
        {
            Console.WriteLine("=== ЗАПУСК МАСШТАБНЫХ ИСПЫТАНИЙ: FLYWEIGHT + PROXY ===");

            var factory = new StandFactory();
            var strategy = new RasterRenderingStrategy();
            var telemetry = ApplicationTelemetrySingleton.Instance;

            // Генерируем 5 эталонных стилей общего пользования
            StyleKey[] sharedStyles = new StyleKey[5];
            for (int i = 0; i < 5; i++)
            {
                sharedStyles[i] = new StyleKey("Segoe UI", 10 + i, (byte)(50 * i), 120, 200);
            }

            // --- 1. Построение дерева масштабирования из 5 000 узлов через фасад ---
            var panelStyle = FlyweightFactory.Instance.GetFlyweight(sharedStyles[0]);
            var rootPanel = new PanelComponent("MegaPanel", new Rectangle(0, 0, 1920, 1080), strategy, panelStyle);

            Stopwatch swColdStart = Stopwatch.StartNew();

            // Добавляем 2000 реальных узлов
            for (int i = 0; i < 2000; i++)
            {
                var style = FlyweightFactory.Instance.GetFlyweight(sharedStyles[i % 5]);
                rootPanel.AddChild(new ButtonComponent($"RealBtn_{i}", new Rectangle(0, 0, 10, 10), "Click", strategy, style));
            }

            // Добавляем 3000 прокси-узлов (Virtual Proxy)
            List<VirtualComponentProxy> proxyList = new();
            for (int i = 0; i < 3000; i++)
            {
                var cfg = new WidgetConfig
                {
                    Type = WidgetType.Label,
                    Id = $"ProxyLabel_{i}",
                    Bounds = new Rectangle(5, 5, 100, 20),
                    Style = sharedStyles[i % 5]
                };
                var proxy = new VirtualComponentProxy(cfg, factory, strategy);
                rootPanel.AddChild(proxy);
                proxyList.Add(proxy);
            }
            swColdStart.Stop();

            // --- 2. Эмуляция работы: материализация ~30% прокси через Render ---
            DefaultRenderingContext context = new();
            int nodesToMaterialize = (int)(proxyList.Count * 0.30);
            for (int i = 0; i < nodesToMaterialize; i++)
            {
                proxyList[i].Render(context);
            }

            // --- 3. Ограничение доступа через ProtectionProxy для 1000 узлов ---
            List<ProtectionComponentProxy> lockedNodes = new();
            for (int i = 0; i < 1000; i++)
            {
                var protectedComp = new ProtectionComponentProxy(proxyList[i]);
                protectedComp.LockComponent();
                lockedNodes.Add(protectedComp);
            }

            // Проверка защитного прокси на исключение
            try
            {
                lockedNodes[0].SetPosition(new Point(99, 99));
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[Успешный тест ProtectionProxy]: Перехвачено исключение защиты: {ex.Message}");
            }

            // --- 4. Проверка интеграции с Prototype (Глубокое клонирование) ---
            var testBtn = new ButtonComponent("ProtoBtn", new Rectangle(0, 0, 5, 5), "Txt", strategy, FlyweightFactory.Instance.GetFlyweight(sharedStyles[0]));
            var clonedBtn = (ButtonComponent)testBtn.Clone();

            bool flyweightShared = ReferenceEquals(testBtn.Flyweight, clonedBtn.Flyweight);
            Console.WriteLine($"[Тест Prototype]: Ссылки на Flyweight идентичны у клонов? {flyweightShared}");

            ExecuteValidationExperiments(sharedStyles, factory, strategy);
        }

        private static void ExecuteValidationExperiments(StyleKey[] styles, IWidgetFactory factory, IRenderingStrategy strategy)
        {
            Console.WriteLine("\n=== ЭКСПЕРИМЕНТАЛЬНАЯ ВАЛИДАЦИЯ МЕТРИК ===");

            // Валидация Flyweight
            FlyweightFactory.Instance.ResetMetrics();
            for (int i = 0; i < 5000; i++)
            {
                FlyweightFactory.Instance.GetFlyweight(styles[i % 5]);
            }
            int instancesWithFactory = FlyweightFactory.Instance.TotalInstancesCreated;
            int instancesWithoutFactory = 5000;
            double savings = (1.0 - ((double)instancesWithFactory / instancesWithoutFactory)) * 100;
            Console.WriteLine($"[Flyweight] Экземпляров без фабрики: {instancesWithoutFactory}, с фабрикой: {instancesWithFactory}");
            Console.WriteLine($"[Flyweight] Сокращение выделения памяти под стили на: {savings:F1}% (Ожидается: 80–95%)");

            // Валидация Proxy холодного старта
            Stopwatch swPure = Stopwatch.StartNew();
            for (int i = 0; i < 2000; i++)
            {
                var style = FlyweightFactory.Instance.GetFlyweight(styles[i % 5]);
                var clean = new LabelComponent($"L_{i}", new Rectangle(0, 0, 10, 10), "Txt", strategy, style);
            }
            swPure.Stop();

            Stopwatch swProxy = Stopwatch.StartNew();
            for (int i = 0; i < 2000; i++)
            {
                var cfg = new WidgetConfig { Type = WidgetType.Label, Id = $"L_{i}", Bounds = new Rectangle(0, 0, 10, 10), Style = styles[i % 5] };
                var p = new VirtualComponentProxy(cfg, factory, strategy);
            }
            swProxy.Stop();

            double speedup = (1.0 - ((double)swProxy.ElapsedTicks / swPure.ElapsedTicks)) * 100;
            Console.WriteLine($"[Proxy] Время мгновенного создания: {swPure.Elapsed.TotalMilliseconds:F3} мс");
            Console.WriteLine($"[Proxy] Время создания через VirtualProxy: {swProxy.Elapsed.TotalMilliseconds:F3} мс");
            Console.WriteLine($"[Proxy] Ускорение холодного старта на: {speedup:F1}% (Ожидается: >=40%)");
        }
    }
}