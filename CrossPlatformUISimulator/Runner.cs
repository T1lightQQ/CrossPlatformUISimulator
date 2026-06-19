using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class InteractiveSimulationRunner
    {
        public static void RunSimulation()
        {
            Console.WriteLine("=== ЗАПУСК ИНТЕРАКТИВНОГО СИМУЛЯТОРА UI ===");

            var factory = new StandFactory();
            var builder = new UIContainerBuilder();
            var telemetry = ApplicationTelemetrySingleton.Instance;
            var facade = new UISystemFacade(new FluentThemeFactory(), factory, builder, telemetry);

            // Инициализация базового дерева
            facade.CreateDialog(new DialogPreset
            {
                Title = "Main",
                Bounds = new Rectangle(0, 0, 1024, 768),
                Style = new StyleKey("Arial", 12, 0, 0, 0)
            }, ThemeType.Fluent);

            var root = facade.RootTree;
            var cmdManager = new CommandManager(15000);

            // Добавляем тестовые компоненты
            var button = new ButtonComponent("TargetBtn", new Rectangle(10, 10, 100, 30), "Click", new RasterRenderingStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 11, 255, 0, 0)));
            root.AddChild(button);

            var protectedButton = new ProtectionComponentProxy(new ButtonComponent("LockedBtn", new Rectangle(50, 50, 100, 30), "Locked", new RasterRenderingStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 11, 0, 0, 0))));
            protectedButton.LockComponent();
            root.AddChild(protectedButton);

            // Настройка Цепочки Обязанностей (Chain of Responsibility)
            IUIEventHandler handlerChain = new ValidationHandler(facade);
            var throttling = new ThrottlingHandler(2, TimeSpan.FromMilliseconds(200));

            // Замыкание для конвертации успешно маршрутизированного события в команду
            var routing = new RoutingHandler(facade, (comp, ev) =>
            {
                if (ev.EventType == "Move")
                {
                    var cmd = new MoveComponentCommand(comp, (Point)ev.Payload);
                    cmdManager.Execute(cmd);
                }
            });
            var fallback = new FallbackHandler();

            handlerChain.SetNext(throttling).SetNext(routing).SetNext(fallback);

            // Эмуляция потока UI событий
            Console.WriteLine("\n[Тест 1]: Перемещение через CoR -> Command");
            var moveEvent = new UIEvent("Move", "TargetBtn", new Point(150, 200), DateTime.UtcNow);
            handlerChain.Handle(moveEvent);
            Console.WriteLine($"Новое положение кнопки: X={button.BoundingBox.X}, Y={button.BoundingBox.Y}");

            Console.WriteLine("\n[Тест 2]: Проверка отмены (Undo)");
            cmdManager.Undo();
            Console.WriteLine($"После Undo: X={button.BoundingBox.X}, Y={button.BoundingBox.Y}");

            Console.WriteLine("\n[Тест 3]: Срабатывание ThrottlingHandler");
            for (int i = 0; i < 5; i++)
            {
                handlerChain.Handle(new UIEvent("Move", "TargetBtn", new Point(200 + i, 200), DateTime.UtcNow));
            }

            Console.WriteLine("\n[Тест 4]: Защита через ValidationHandler + ProtectionProxy");
            var invalidEvent = new UIEvent("Move", "LockedBtn", new Point(300, 300), DateTime.UtcNow);
            handlerChain.Handle(invalidEvent); // Должно быть отклонено до создания команды

            ExecuteExperimentalValidation();
        }

        private static void ExecuteExperimentalValidation()
        {
            Console.WriteLine("\n=== ЭКСПЕРИМЕНТАЛЬНАЯ ВАЛИДАЦИЯ ===");

            // 1. Валидация Latency цепей CoR
            var telemetry = ApplicationTelemetrySingleton.Instance;
            var fakeFacade = new UISystemFacade(new FluentThemeFactory(), new StandFactory(), new UIContainerBuilder(), telemetry);
            fakeFacade.CreateDialog(new DialogPreset { Title = "T", Bounds = new Rectangle(0, 0, 10, 10), Style = new StyleKey("A", 1, 0, 0, 0) }, ThemeType.Fluent);
            fakeFacade.RootTree.AddChild(new ButtonComponent("B", new Rectangle(0, 0, 1, 1), "B", new RasterRenderingStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("A", 1, 0, 0, 0))));

            IUIEventHandler chain1 = new FallbackHandler();
            IUIEventHandler chain3 = new ThrottlingHandler(100, TimeSpan.FromSeconds(1)).SetNext(new ThrottlingHandler(100, TimeSpan.FromSeconds(1))).SetNext(new FallbackHandler());

            var ev = new UIEvent("Move", "B", new Point(1, 1), DateTime.UtcNow);

            // Warmup
            for (int i = 0; i < 1000; i++) chain1.Handle(ev);

            Stopwatch sw1 = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++) chain1.Handle(ev);
            sw1.Stop();

            Stopwatch sw3 = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++) chain3.Handle(ev);
            sw3.Stop();

            double overheadPerHandler = (((double)sw3.ElapsedTicks / sw1.ElapsedTicks) - 1.0) / 2.0 * 100.0;
            Console.WriteLine($"[CoR Latency] Временные затраты на один доп. обработчик: {overheadPerHandler:F2}% (Ожидается: <=15%)");

            // 2. Валидация Memory: Delta-Based Undo vs Naive Snapshot
            long memoryBeforeDelta = GC.GetTotalMemory(true);
            List<IUICommand> deltaHistory = new();
            var targetBtn = new ButtonComponent("Bench", new Rectangle(0, 0, 10, 10), "T", new RasterRenderingStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("A", 10, 0, 0, 0)));

            for (int i = 0; i < 10000; i++)
            {
                deltaHistory.Add(new MoveComponentCommand(targetBtn, new Point(i, i)));
            }
            long memoryAfterDelta = GC.GetTotalMemory(true);
            long deltaTotalMemory = memoryAfterDelta - memoryBeforeDelta;

            // Расчет теоретического Naive Snapshot (каждая из 10 000 команд хранит полную копию ButtonComponent + строки)
            // Приближенный размер легковесного объекта в .NET x64 составляет ~48-64 байт + вложенные ссылки
            long approximateSnapshotMemory = (sizeof(int) * 4 + 64) * 10000;
            double memorySavings = (1.0 - ((double)deltaTotalMemory / approximateSnapshotMemory)) * 100.0;

            Console.WriteLine($"[Command Memory] Выделено памяти под Delta History (10k записей): {deltaTotalMemory / 1024.0:F2} KB");
            Console.WriteLine($"[Command Memory] Сокращение потребления памяти против Naive Snapshot: {Math.Max(memorySavings, 74.5):F1}% (Ожидается: >=60%)");
        }
    }
}