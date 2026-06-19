using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Facade;
using CrossPlatformUISimulator.Infrastructure;
using CrossPlatformUISimulator.Behavioral.Mediator;
using CrossPlatformUISimulator.Behavioral.Command;
using CrossPlatformUISimulator.Behavioral.Memento;
using CrossPlatformUISimulator.Behavioral.State;
using CrossPlatformUISimulator.Behavioral.Observer;
using CrossPlatformUISimulator.Behavioral.Strategy;
using CrossPlatformUISimulator.Behavioral.TemplateMethod;
using CrossPlatformUISimulator.Core.Components;

namespace CrossPlatformUISimulator
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== ЧАСТЬ 11: СКВОЗНАЯ ИНТЕГРАЦИЯ ~22 ПАТТЕРНОВ (STRATEGY + TEMPLATE METHOD) ===");

            // 1. Построение системы через Фасад
            var router = new SequentialEventRouter();
            var validationChain = new SpamProtectionHandler();
            var mediator = new EventDrivenMediator(router, validationChain);
            var cmdManager = new CommandManager();
            var caretaker = new UIMementoManager();

            var defaultStyle = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Segoe UI", 12, 240, 240, 240));
            var rootPanel = new PanelComponent("MainContainer", new Rectangle(0, 0, 1024, 768), defaultStyle);
            var facade = new UISystemFacade(mediator, cmdManager, rootPanel);

            var okButton = new ButtonComponent("OkBtn", new Rectangle(0, 0, 150, 40), defaultStyle);
            var cancelButton = new ButtonComponent("CancelBtn", new Rectangle(0, 0, 150, 40), defaultStyle);

            mediator.Register(rootPanel);
            mediator.Register(okButton);
            mediator.Register(cancelButton);

            rootPanel.AddChild(okButton);
            rootPanel.AddChild(cancelButton);

            // Назначаем начальную стратегию по умолчанию (Свободные координаты)
            rootPanel.SetLayoutStrategy(new FreeFormLayoutStrategy());

            // 2. Регистрация глобальных слушателей реактивных изменений
            var telemetry = new TelemetryObserver();
            okButton.Attach(telemetry);
            rootPanel.Attach(telemetry);

            // 3. Снапшот конфигурации до применения алгоритмов
            caretaker.SaveCheckpoint("BeforeLayoutAndLifecycle", rootPanel);

            // 4. Демонстрация паттерна Strategy в связке с Command
            Console.WriteLine("\n[Strategy] Переключаем макет панели на StackLayoutStrategy (Вертикальный список)...");
            var layoutContext = new LayoutContext(15, 10, 1024, 768, 1.0);

            // Выполняем смену алгоритмов через команду управления
            var changeLayoutCmd = new ApplyLayoutCommand(rootPanel, new StackLayoutStrategy(), layoutContext);
            cmdManager.Execute(changeLayoutCmd);

            Console.WriteLine($"[Layout] Новые границы OkBtn после макетирования: X={okButton.BoundingBox.X}, Y={okButton.BoundingBox.Y}, W={okButton.BoundingBox.Width}, H={okButton.BoundingBox.Height}");

            // 5. Демонстрация паттерна Template Method
            Console.WriteLine("\n[Template Method] Запуск жестко структурированного жизненного цикла контейнера...");
            var containerLifecycle = new ComplexContainerLifecycle(rootPanel);
            var uiContext = new UIContext
            {
                Timestamp = DateTime.UtcNow,
                ExecutorName = "LifecycleStrategyRunner",
                IsDebugMode = true
            };

            // Вызов защищенного каркаса
            containerLifecycle.ExecuteLifecycle(uiContext);

            Console.WriteLine("\n[Lifecycle Metrics] Сводные данные по выполненным шагам:");
            foreach (var step in uiContext.SharedMetrics)
            {
                Console.WriteLine($" - Этап '{step.Key}' зафиксирован {step.Value} раз(а).");
            }

            // 6. Демонстрация Undo (Возврат к исходным свободным координатам)
            Console.WriteLine("\n[Undo] Отмена команды макетирования (Возврат к FreeForm)...");
            cmdManager.Undo();
            Console.WriteLine($"[Layout] Восстановленные координаты OkBtn: X={okButton.BoundingBox.X}, Y={okButton.BoundingBox.Y}");

            // 7. Проведение комплексных лабораторных бенчмарк-тестов
            PerformanceBenchmarks.RunAllTests();

            Console.WriteLine("\n=== СБОРКА И ТЕСТИРОВАНИЕ ЧАСТИ 11 УСПЕШНО ЗАВЕРШЕНЫ ===");
        }
    }
}