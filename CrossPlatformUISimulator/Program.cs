using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Facade;
using CrossPlatformUISimulator.Infrastructure;
using CrossPlatformUISimulator.Behavioral.Mediator;
using CrossPlatformUISimulator.Behavioral.Command;
using CrossPlatformUISimulator.Behavioral.Memento;
using CrossPlatformUISimulator.Behavioral.State;
using CrossPlatformUISimulator.Behavioral.Observer;
using CrossPlatformUISimulator.Core.Components;

namespace CrossPlatformUISimulator
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== ЧАСТЬ 10: СИНТЕНЗ ВСЕХ ~20 ПАТТЕРНОВ (STATE + OBSERVER) ===");

            // 1. Конфигурация подсистем
            var router = new SequentialEventRouter();
            var validationChain = new SpamProtectionHandler();
            var mediator = new EventDrivenMediator(router, validationChain);
            var cmdManager = new CommandManager();
            var caretaker = new UIMementoManager();

            var defaultStyle = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Segoe UI", 12, 240, 240, 240));
            var rootPanel = new PanelComponent("MainWindow", new Rectangle(0, 0, 1024, 768), defaultStyle);
            var facade = new UISystemFacade(mediator, cmdManager, rootPanel);

            var actionButton = new ButtonComponent("SubmitBtn", new Rectangle(50, 50, 200, 50), defaultStyle);
            mediator.Register(rootPanel);
            mediator.Register(actionButton);
            rootPanel.AddChild(actionButton);

            // 2. Реактивная интеграция (Observer)
            var telemetryObs = new TelemetryObserver();
            var themeObs = new ThemeSyncObserver();
            actionButton.Attach(telemetryObs);
            actionButton.Attach(themeObs);

            // 3. Фиксация стабильного снапшота (Memento)
            caretaker.SaveCheckpoint("StableSnapshot", rootPanel);

            // 4. Демонстрация изменения поведения (State)
            Console.WriteLine($"\n[State] Текущее состояние кнопки: {actionButton.CurrentState.StateName}");
            Console.WriteLine("[User] Инициируем клик в NormalState:");
            actionButton.UserClick(); // Сработает Медиатор

            Console.WriteLine("\n[Command] Переводим кнопку в LoadingState через команду...");
            var loadingCmd = new StartLoadingCommand(actionButton);
            cmdManager.Execute(loadingCmd); // Trigger State Change -> Notify Observers

            Console.WriteLine($"\n[State] Текущее состояние кнопки: {actionButton.CurrentState.StateName}");
            Console.WriteLine("[User] Пытаемся нажать кнопку во время загрузки:");
            actionButton.UserClick(); // Клик заблокирован полиморфным LoadingState (NoOp)

            // 5. Использование Undo (Возврат к Normal)
            Console.WriteLine("\n[Undo] Отмена команды загрузки...");
            cmdManager.Undo();
            Console.WriteLine($"[State] Состояние после отмены: {actionButton.CurrentState.StateName}");

            // 6. Искусственный перевод в аварийный режим
            Console.WriteLine("\n[Command] Перевод в аварийный режим...");
            cmdManager.Execute(new TriggerErrorCommand(actionButton));
            Console.WriteLine($"[State] Состояние: {actionButton.CurrentState.StateName}, Текст: {actionButton.TextContent}");

            // 7. Полный откат иерархии через Топологический Memento Снапшот
            Console.WriteLine("\n[Memento] Восстановление системы к первоначальному чекпоинту...");
            caretaker.RestoreCheckpoint("StableSnapshot", rootPanel);
            Console.WriteLine($"[State] Финальное состояние кнопки: {actionButton.CurrentState.StateName}, Текст: {actionButton.TextContent}");

            // 8. Запуск нагрузочного профилирования производительности
            PerformanceBenchmarks.RunAllTests();
        }
    }
}