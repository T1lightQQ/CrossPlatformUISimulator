using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Facade;
using CrossPlatformUISimulator.Infrastructure;
using CrossPlatformUISimulator.Behavioral.Mediator;
using CrossPlatformUISimulator.Behavioral.Command;
using CrossPlatformUISimulator.Behavioral.Memento;
using CrossPlatformUISimulator.Behavioral.Strategy;
using CrossPlatformUISimulator.Behavioral.Visitor;
using CrossPlatformUISimulator.Core.Components;

namespace CrossPlatformUISimulator
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("==========================================================================");
            Console.WriteLine("🚀 ФИНАЛ: ПОЛНЫЙ ЦИКЛ ИНТЕГРАЦИИ ВСЕХ ~23 ПАТТЕРНОВ ПРОЕКТИРОВАНИЯ");
            Console.WriteLine("==========================================================================");

            // 1. Инициализация инфраструктурных синглтонов и шин обмена данных
            var router = new SequentialEventRouter();
            var validationChain = new SpamProtectionHandler();
            var mediator = new EventDrivenMediator(router, validationChain);
            var cmdManager = new CommandManager();
            var mementoCaretaker = new UIMementoManager();

            // 2. Сборка Composite-структуры с использованием Flyweight-стилей
            var defaultStyle = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Ubuntu Sans", 11, 30, 30, 30));
            var rootContainer = new PanelComponent("WorkspaceRoot", new Rectangle(0, 0, 1920, 1080), defaultStyle);
            var facade = new UISystemFacade(mediator, cmdManager, rootContainer);

            var actionButton = new ButtonComponent("SubmitAction", new Rectangle(10, 10, 200, 50), defaultStyle);
            mediator.Register(rootContainer);
            mediator.Register(actionButton);
            rootContainer.AddChild(actionButton);

            // Назначаем базовую стратегию размещения
            rootContainer.SetLayoutStrategy(new StackLayoutStrategy());

            // 3. Фиксация начального состояния (Memento Checkpoint) перед анализом
            Console.WriteLine("\n[Snapshot] Сохраняем эталонную конфигурацию до прохода посетителей...");
            mementoCaretaker.SaveCheckpoint("InitialState", rootContainer);

            // 4. Запуск аналитических систем (Паттерн Visitor)
            Console.WriteLine("\n[Visitor Analysis] Выполняем кросс-каттинг аудит дерева компонентов...");

            var metricsVisitor = new MetricsCollectorVisitor();
            var accessibilityVisitor = new AccessibilityTreeVisitor();
            var validatorVisitor = new DependencyValidatorVisitor();

            facade.RunVisitor(metricsVisitor);
            facade.RunVisitor(accessibilityVisitor);
            facade.RunVisitor(validatorVisitor);

            // Вывод собранных данных
            var report = metricsVisitor.GetReport();
            Console.WriteLine($" -> [Метрики] Всего узлов в графе: {report.TotalNodes}, Различных гарнитур: {report.DistinctStylesCount}");
            Console.WriteLine($" -> [Доступность] Сгенерировано логических узлов для скринридеров: {accessibilityVisitor.AccessibleNodes.Count}");

            if (validatorVisitor.ValidationErrors.Count == 0)
            {
                Console.WriteLine(" -> [Валидация] Архитектурные инварианты соблюдены, конфликтов State/Flyweight не обнаружено.");
            }
            else
            {
                foreach (var err in validatorVisitor.ValidationErrors) Console.WriteLine($" -> {err}");
            }

            // 5. Симуляция деструктивного действия пользователя и проверка транзакционности
            Console.WriteLine("\n[Interaction] Смена макета через команду и проверка работы механизма Undo...");
            var layoutCtx = new LayoutContext(20, 5, 1920, 1080, 1.2);
            facade.ApplyLayout("WorkspaceRoot", new FreeFormLayoutStrategy(), layoutCtx);

            // Возвращаем все параметры обратно, используя менеджер команд
            Console.WriteLine("[Undo] Откат транзакции. Восстановление исходной геометрии дерева.");
            cmdManager.Undo();

            // 6. Запуск комплексных лабораторных бенчмарков производительности
            PerformanceBenchmarks.RunAllTests();

            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("🏆 СИМУЛЯТОР УСПЕШНО СКОМПИЛИРОВАН И ЗАВЕРШИЛ РАБОТУ БЕЗ ОШИБОК!");
            Console.WriteLine("==========================================================================");
        }
    }
}