using System;
using System.Diagnostics;
using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Core.Components;
using CrossPlatformUISimulator.Behavioral.State;
using CrossPlatformUISimulator.Behavioral.Observer;
using CrossPlatformUISimulator.Behavioral.Strategy;

namespace CrossPlatformUISimulator.Infrastructure
{
    public static class PerformanceBenchmarks
    {
        public static void RunAllTests()
        {
            Console.WriteLine("\n=== ЗАПУСК ЭКСПЕРИМЕНТАЛЬНОЙ ВАЛИДАЦИИ (ЧАСТЬ 30-33) ===");
            BenchmarkObserverLatency();
            BenchmarkStateVsSwitch();
            BenchmarkStrategyDispatch();
            BenchmarkTemplateMethodVsComposition();
        }

        private static void BenchmarkObserverLatency()
        {
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 12, 0, 0, 0));
            var button = new ButtonComponent("BenchBtn", new Rectangle(0, 0, 10, 10), style);

            int subscriberCount = 10000;
            for (int i = 0; i < subscriberCount; i++)
            {
                button.Attach(new TelemetryObserver());
            }

            button.Notify(new UIStateChangeData("Warmup", "A", "B", DateTime.UtcNow));

            var sw = Stopwatch.StartNew();
            button.Notify(new UIStateChangeData("LatencyTest", "NormalState", "LoadingState", DateTime.UtcNow));
            sw.Stop();

            Console.WriteLine($"[Benchmark] Dispatch latency для {subscriberCount} подписчиков: {sw.Elapsed.TotalMilliseconds:F4} ms");
            Console.WriteLine($"[Overhead] Накладные расходы на одного подписчика: {(sw.Elapsed.TotalMilliseconds * 1000000 / subscriberCount):F2} ns");
        }

        private static void BenchmarkStateVsSwitch()
        {
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 12, 0, 0, 0));
            var button = new ButtonComponent("StateBench", new Rectangle(0, 0, 10, 10), style);
            int iterations = 100000;

            var swState = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                button.TransitionTo(LoadingState.Instance);
                button.TransitionTo(NormalState.Instance);
            }
            swState.Stop();

            var swSwitch = Stopwatch.StartNew();
            WidgetType currentType = WidgetType.Button;
            for (int i = 0; i < iterations; i++)
            {
                string res1 = currentType switch { WidgetType.Button => "Normal", _ => "Unknown" };
                string res2 = currentType switch { WidgetType.Button => "Loading", _ => "Unknown" };
            }
            swSwitch.Stop();

            double diffPercent = ((double)(swState.ElapsedTicks - swSwitch.ElapsedTicks) / swSwitch.ElapsedTicks) * 100;
            Console.WriteLine($"[Benchmark] State pattern ({iterations} переходов): {swState.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"[Benchmark] Switch Expression ({iterations} проверок): {swSwitch.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"[Overhead] Разница виртуальных вызовов (State vs Switch): +{diffPercent:F2}%");
        }

        // ЧАСТЬ 33: Сравнение интерфейсной стратегии с распределением через делегаты Func<>
        private static void BenchmarkStrategyDispatch()
        {
            int iterations = 50000;
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 11, 0, 0, 0));
            var panel = new PanelComponent("BenchPanel", new Rectangle(0, 0, 100, 100), style);
            var ctx = new LayoutContext(5, 2, 500, 500, 1.0);

            ILayoutStrategy interfaceStrategy = new StackLayoutStrategy();
            Func<IContainerComponent, LayoutContext, IReadOnlyDictionary<string, Rectangle>> delegateStrategy =
                (c, l) => interfaceStrategy.CalculateBounds(c, l);

            // Warmup
            interfaceStrategy.CalculateBounds(panel, ctx);
            delegateStrategy(panel, ctx);

            var swInterface = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                interfaceStrategy.CalculateBounds(panel, ctx);
            }
            swInterface.Stop();

            var swDelegate = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                delegateStrategy(panel, ctx);
            }
            swDelegate.Stop();

            double overhead = ((double)(swInterface.ElapsedTicks - swDelegate.ElapsedTicks) / swDelegate.ElapsedTicks) * 100;
            Console.WriteLine($"[Benchmark] Вызов через интерфейс Strategy: {swInterface.Elapsed.TotalMilliseconds:F3} ms");
            Console.WriteLine($"[Benchmark] Вызов через делегат Func<>: {swDelegate.Elapsed.TotalMilliseconds:F3} ms");
            Console.WriteLine($"[Overhead] Накладные расходы интерфейса vtable: {overhead:F2}% (Ожидается <= 5-8%)");
        }

        // ЧАСТЬ 33: Оценка эффективности Template Method (Наследование) против Композиции + Делегатов
        private static void BenchmarkTemplateMethodVsComposition()
        {
            int iterations = 50000;
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 11, 0, 0, 0));
            var btn = new ButtonComponent("TBtn", new Rectangle(0, 0, 50, 50), style);
            var uiCtx = new UIContext { Timestamp = DateTime.UtcNow, ExecutorName = "Benchmark" };

            var templateLifecycle = new Behavioral.TemplateMethod.StandardComponentLifecycle(btn);

            // Имитация композиции шагов (Pipeline-подход)
            Action<UIContext> compositionPipeline = (c) => {
                // Инициализация -> Валидация -> Рендеринг
                bool valid = btn.CurrentState.StateName != "ErrorState";
                if (valid) { /* Render */ }
            };

            // Warmup
            templateLifecycle.ExecuteLifecycle(uiCtx);
            compositionPipeline(uiCtx);

            var swTemplate = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                templateLifecycle.ExecuteLifecycle(uiCtx);
            }
            swTemplate.Stop();

            var swComposition = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                compositionPipeline(uiCtx);
            }
            swComposition.Stop();

            double overhead = ((double)(swTemplate.ElapsedTicks - swComposition.ElapsedTicks) / swComposition.ElapsedTicks) * 100;
            Console.WriteLine($"[Benchmark] Исполнение через Template Method: {swTemplate.Elapsed.TotalMilliseconds:F3} ms");
            Console.WriteLine($"[Benchmark] Исполнение через Композицию шагов: {swComposition.Elapsed.TotalMilliseconds:F3} ms");
            Console.WriteLine($"[Overhead] Издержки глубокого наследования: +{overhead:F2}% (Ожидается <= 3%)");
        }
    }
}