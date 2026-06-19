using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class DslSimulationRunner
    {
        public static void Main()
        {
            Console.WriteLine("=== ЗАПУСК СКВОЗНОЙ СИМУЛЯЦИИ UI-DSL И ИТЕРАТОРОВ ===");

            var builder = new UIContainerBuilder();
            var cmdManager = new CommandManager();
            var facade = new UISystemFacade(builder, cmdManager);

            facade.CreateDialog(new DialogPreset
            {
                Title = "Workspace",
                Bounds = new Rectangle(0, 0, 1920, 1080)
            }, ThemeType.Fluent);

            var rootTree = facade.RootTree;

            // Наполнение дерева тестовыми паттернами
            var btn1 = new ButtonComponent("Btn_01", new Rectangle(10, 10, 100, 30), new DummyRasterStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 11, 200, 200, 200))) { Enabled = false };
            var btn2 = new ButtonComponent("Btn_02", new Rectangle(15, 50, 100, 30), new DummyRasterStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 11, 200, 200, 200))) { Enabled = false };

            var lbl1 = new LabelComponent("Lbl_01", new Rectangle(200, 10, 50, 20), new DummyRasterStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 10, 0, 0, 0)));

            // Декорирование и проксирование узлов
            var borderDecoratedBtn = new BorderDecorator(btn1);
            var unmaterializedProxy = new VirtualComponentProxy("LazyBtn", new Rectangle(0, 0, 0, 0)); // Не должен обходиться DFS-итератором

            rootTree.AddChild(borderDecoratedBtn);
            rootTree.AddChild(btn2);
            rootTree.AddChild(lbl1);
            rootTree.AddChild(unmaterializedProxy);

            // Исполнение декларативного DSL выражения
            string dslScript = "SELECT <Button> WHERE Enabled=false -> EXECUTE ApplyTheme('Fluent') -> SetPosition(10,20)";
            Console.WriteLine($"\nВыполнение DSL Скрипта: {dslScript}");

            facade.ExecuteDsl(dslScript);

            Console.WriteLine("Атомарные изменения применены. Проверка координат Btn_02:");
            Console.WriteLine($"Позиция Btn_02 после DSL: X={btn2.BoundingBox.X}, Y={btn2.BoundingBox.Y} (Ожидается 10, 20)");

            Console.WriteLine("\nТестирование механизма отмены транзакции (Undo)...");
            cmdManager.Undo();
            Console.WriteLine($"Позиция Btn_02 после Undo: X={btn2.BoundingBox.X}, Y={btn2.BoundingBox.Y} (Ожидается возврат к исходным 15, 50)");

            ExecuteExperimentalValidation();
        }

        private static void ExecuteExperimentalValidation()
        {
            Console.WriteLine("\n=== ЭКСПЕРИМЕНТАЛЬНАЯ ВАЛИДАЦИЯ МЕТРИК ===");

            // 1. Валидация производительности ручного итератора против yield return на 10,000 узлов
            var benchmarkRoot = new PanelComponent("BenchRoot", new Rectangle(0, 0, 1, 1), new DummyRasterStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("A", 1, 1, 1, 1)));
            for (int i = 0; i < 10000; i++)
            {
                benchmarkRoot.AddChild(new ButtonComponent($"B_{i}", new Rectangle(0, 0, 1, 1), new DummyRasterStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("A", 1, 1, 1, 1))));
            }

            // Разогрев JIT
            var manualIt = new DepthFirstIterator(benchmarkRoot);
            while (manualIt.MoveNext()) { _ = manualIt.Current; }

            // Измерение ручного конечного автомата (DFS)
            var swManual = Stopwatch.StartNew();
            for (int r = 0; r < 50; r++)
            {
                manualIt.Reset();
                while (manualIt.MoveNext()) { _ = manualIt.Current; }
            }
            swManual.Stop();

            // Измерение yield return эквивалента
            var swYield = Stopwatch.StartNew();
            for (int r = 0; r < 50; r++)
            {
                foreach (var node in TraverseWithYield(benchmarkRoot)) { _ = node; }
            }
            swYield.Stop();

            double overhead = ((double)swManual.ElapsedTicks / swYield.ElapsedTicks - 1.0) * 100.0;
            Console.WriteLine($"[Iterator Performance] Накладные расходы ручного стейт-машина: {overhead:F2}% (Целевой инвариант: <=25%)");

            // 2. Тестирование пропускной способности Интерпретатора (1,000 проходов)
            var builder = new UIContainerBuilder();
            var cmdManager = new CommandManager();
            var facade = new UISystemFacade(builder, cmdManager);
            facade.CreateDialog(new DialogPreset { Title = "T", Bounds = new Rectangle(0, 0, 1, 1) }, ThemeType.Fluent);
            facade.RootTree.AddChild(new ButtonComponent("B", new Rectangle(0, 0, 1, 1), new DummyRasterStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("A", 1, 1, 1, 1))));

            string dsl = "SELECT <Button> WHERE Visible=true -> EXECUTE SetPosition(1,1)";

            long memBefore = GC.GetTotalMemory(true);
            var swDsl = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                facade.ExecuteDsl(dsl);
            }
            swDsl.Stop();
            long memAfter = GC.GetTotalMemory(true);

            Console.WriteLine($"[Interpreter Throughput] Время разбора и выполнения 1000 DSL скриптов: {swDsl.ElapsedMilliseconds} ms");
            Console.WriteLine($"[Interpreter Memory] Аллокационное давление парсера графа AST: {(memAfter - memBefore) / 1024.0:F2} KB");
        }

        private static IEnumerable<IUIComponent> TraverseWithYield(IUIComponent root)
        {
            var stack = new Stack<IUIComponent>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current is ILazyComponentProxy proxy && !proxy.IsMaterialized) continue;
                yield return current;

                if (current is UIComponentDecorator dec) stack.Push(dec.GetWrappedComponent());
                else if (current is IContainerComponent container)
                {
                    for (int i = container.Children.Count - 1; i >= 0; i--) stack.Push(container.Children[i]);
                }
            }
        }
    }
}