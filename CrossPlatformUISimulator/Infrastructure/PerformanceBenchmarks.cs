using System;
using System.Diagnostics;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Core.Components;
using CrossPlatformUISimulator.Infrastructure;
using CrossPlatformUISimulator.Behavioral.State;
using CrossPlatformUISimulator.Behavioral.Observer;

namespace CrossPlatformUISimulator.Infrastructure
{
    public static class PerformanceBenchmarks
    {
        public static void RunAllTests()
        {
            Console.WriteLine("\n=== ЗАПУСК ЭКСПЕРИМЕНТАЛЬНОЙ ВАЛИДАЦИИ (ЧАСТЬ 30) ===");
            BenchmarkObserverLatency();
            BenchmarkStateVsSwitch();
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

            // Warmup
            button.Notify(new UIStateChangeData("Warmup", "A", "B", DateTime.UtcNow));

            var sw = Stopwatch.StartNew();
            button.Notify(new UIStateChangeData("LatencyTest", "NormalState", "LoadingState", DateTime.UtcNow));
            sw.Stop();

            Console.WriteLine($"[Benchmark] Dispatch latency для {subscriberCount} подписчиков: {sw.Elapsed.TotalMilliseconds:F4} ms");
            Console.WriteLine($"[Overhead] Накладные расходы на одного подписчика: {(sw.Elapsed.TotalMilliseconds * 1000000 / subscriberCount):F2} ns (Ожидаемый лимит: <= 15% overhead)");
        }

        private static void BenchmarkStateVsSwitch()
        {
            var style = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 12, 0, 0, 0));
            var button = new ButtonComponent("StateBench", new Rectangle(0, 0, 10, 10), style);
            int iterations = 100000;

            // 1. Полиморфный паттерн State
            var swState = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                button.TransitionTo(LoadingState.Instance);
                button.TransitionTo(NormalState.Instance);
            }
            swState.Stop();

            // 2. Симуляция Switch Expression на enum
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
            Console.WriteLine($"[Overhead] Разница виртуальных вызовов: +{diffPercent:F2}% (Ожидаемый overhead: <= 5-10% в JIT inline)");
        }
    }
}