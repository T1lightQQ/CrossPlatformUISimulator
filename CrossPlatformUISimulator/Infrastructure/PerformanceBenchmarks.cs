using System;
using System.Diagnostics;
using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Core.Components;
using CrossPlatformUISimulator.Core.Decorators;
using CrossPlatformUISimulator.Core.Proxies;
using CrossPlatformUISimulator.Behavioral.State;
using CrossPlatformUISimulator.Behavioral.Observer;
using CrossPlatformUISimulator.Behavioral.Strategy;
using CrossPlatformUISimulator.Behavioral.Visitor;

namespace CrossPlatformUISimulator.Infrastructure
{
    public static class PerformanceBenchmarks
    {
        public static void RunAllTests()
        {
            Console.WriteLine("\n=== ЗАПУСК ЭКСПЕРИМЕНТАЛЬНОЙ ВАЛИДАЦИИ СИСТЕМЫ ===");
            BenchmarkObserverLatency();
            BenchmarkStateVsSwitch();
            BenchmarkStrategyDispatch();
            BenchmarkTemplateMethodVsComposition();
            BenchmarkVisitorVsPatternMatching();
        }

        private static void BenchmarkObserverLatency()
        {
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 12, 0, 0, 0));
            var button = new ButtonComponent("BenchBtn", new Rectangle(0, 0, 10, 10), style);

            int subscriberCount = 1000;
            for (int i = 0; i < subscriberCount; i++)
            {
                button.Attach(new TelemetryObserver());
            }

            button.Notify(new UIStateChangeData("Warmup", "A", "B", DateTime.UtcNow));

            var sw = Stopwatch.StartNew();
            button.Notify(new UIStateChangeData("LatencyTest", "NormalState", "LoadingState", DateTime.UtcNow));
            sw.Stop();

            Console.WriteLine($"[Benchmark] Dispatch latency для {subscriberCount} подписчиков: {sw.Elapsed.TotalMilliseconds:F4} ms");
        }

        private static void BenchmarkStateVsSwitch()
        {
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 12, 0, 0, 0));
            var button = new ButtonComponent("StateBench", new Rectangle(0, 0, 10, 10), style);
            int iterations = 10000;

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
            Console.WriteLine($"[Benchmark] Разница вызовов (State vs Switch): +{diffPercent:F2}%");
        }

        private static void BenchmarkStrategyDispatch()
        {
            int iterations = 10000;
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 11, 0, 0, 0));
            var panel = new PanelComponent("BenchPanel", new Rectangle(0, 0, 100, 100), style);
            var ctx = new LayoutContext(5, 2, 500, 500, 1.0);

            ILayoutStrategy interfaceStrategy = new StackLayoutStrategy();
            Func<IContainerComponent, LayoutContext, IReadOnlyDictionary<string, Rectangle>> delegateStrategy =
                (c, l) => interfaceStrategy.CalculateBounds(c, l);

            var swInterface = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) { interfaceStrategy.CalculateBounds(panel, ctx); }
            swInterface.Stop();

            var swDelegate = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) { delegateStrategy(panel, ctx); }
            swDelegate.Stop();

            double overhead = ((double)(swInterface.ElapsedTicks - swDelegate.ElapsedTicks) / swDelegate.ElapsedTicks) * 100;
            Console.WriteLine($"[Benchmark] Издержки интерфейса vtable в Strategy: {overhead:F2}% (Ожидается <= 5-8%)");
        }

        private static void BenchmarkTemplateMethodVsComposition()
        {
            int iterations = 10000;
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 11, 0, 0, 0));
            var btn = new ButtonComponent("TBtn", new Rectangle(0, 0, 50, 50), style);
            var uiCtx = new UIContext { Timestamp = DateTime.UtcNow, ExecutorName = "Benchmark" };

            var templateLifecycle = new Behavioral.TemplateMethod.StandardComponentLifecycle(btn);
            Action<UIContext> compositionPipeline = (c) => {
                bool valid = btn.CurrentState.StateName != "ErrorState";
            };

            var swTemplate = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) { templateLifecycle.ExecuteLifecycle(uiCtx); }
            swTemplate.Stop();

            var swComposition = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) { compositionPipeline(uiCtx); }
            swComposition.Stop();

            double overhead = ((double)(swTemplate.ElapsedTicks - swComposition.ElapsedTicks) / swComposition.ElapsedTicks) * 100;
            Console.WriteLine($"[Benchmark] Издержки наследования в Template Method: +{overhead:F2}% (Ожидается <= 3%)");
        }

        
        private static void BenchmarkVisitorVsPatternMatching()
        {
            int iterations = 5000;
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 12, 0, 0, 0));

            
            var list = new List<IUIComponent>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(new ButtonComponent($"B{i}", new Rectangle(0, 0, 10, 10), style));
            }

            var visitor = new MetricsCollectorVisitor();

           
            var swVisitor = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var item in list)
                {
                    item.Accept(visitor);
                }
            }
            swVisitor.Stop();

            
            var swPattern = Stopwatch.StartNew();
            int patternCounter = 0;
            for (int i = 0; i < iterations; i++)
            {
                foreach (var item in list)
                {
                    _ = item switch
                    {
                        ButtonComponent b => patternCounter++,
                        PanelComponent p => patternCounter++,
                        UIComponentDecorator d => patternCounter++,
                        VirtualComponentProxy pr => patternCounter++,
                        _ => patternCounter++
                    };
                }
            }
            swPattern.Stop();

            double overhead = ((double)(swVisitor.ElapsedTicks - swPattern.ElapsedTicks) / swPattern.ElapsedTicks) * 100;
            Console.WriteLine($"[Benchmark] Обход через Visitor (Double Dispatch): {swVisitor.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"[Benchmark] Обход через C# Pattern Matching (Switch): {swPattern.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"[Overhead] Накладные расходы двойной диспетчеризации: +{overhead:F2}% (Ожидается <= 10-15%)");
        }
    }
}