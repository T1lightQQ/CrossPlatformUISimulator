using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Facade;
using CrossPlatformUISimulator.Infrastructure;
using CrossPlatformUISimulator.Behavioral.Mediator;
using CrossPlatformUISimulator.Behavioral.Command;
using CrossPlatformUISimulator.Behavioral.Memento;
using CrossPlatformUISimulator.Core.Components;
using CrossPlatformUISimulator.Core.Decorators;

namespace CrossPlatformUISimulator
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== ЗАПУСК: МОДУЛЬНАЯ АРХИТЕКТУРА УСПЕШНО ИНИЦИАЛИЗИРОВАНА ===");

            // 1. Инициализация инфраструктуры и поведенческого слоя
            var router = new SequentialEventRouter();
            var validationChain = new SpamProtectionHandler();
            var mediator = new EventDrivenMediator(router, validationChain);
            var cmdManager = new CommandManager();
            var caretaker = new UIMementoManager();

            // 2. Создание корневых элементов UI через Легковесы
            var defaultStyle = FlyweightFactory.Instance.GetFlyweight(new StyleKey("Segoe UI", 12, 240, 240, 240));
            var rootPanel = new PanelComponent("MainWindow", new Rectangle(0, 0, 1024, 768), defaultStyle);

            // Заворачиваем всё в Фасад
            var systemFacade = new UISystemFacade(mediator, cmdManager, rootPanel);

            // 3. Наполнение дерева и регистрация в Медиаторе
            var dangerButton = new ButtonComponent("DeleteBtn", new Rectangle(20, 20, 120, 40), defaultStyle) { Enabled = false };
            mediator.Register(rootPanel);
            mediator.Register(dangerButton);

            // Декорируем кнопку рамкой и добавляем на панель
            rootPanel.AddChild(new BorderDecorator(dangerButton));

            // 4. Логирование событий
            systemFacade.Subscribe(
                e => e.EventType == "UI_Click",
                e => Console.WriteLine($"[ФАСАД ПЕРЕХВАТ] Нажата кнопка: {e.Payload}")
            );

            // 5. Тестирование Снапшота (Memento)
            Console.WriteLine("\n[Caretaker] Делаем снимок системы 'v1.0'...");
            caretaker.SaveCheckpoint("v1.0", rootPanel);

            // 6. Выполнение скрипта DSL
            string script = "SELECT <Button> WHERE Enabled=false -> EXECUTE ApplyTheme('Fluent') -> SetPosition(10,20)";
            Console.WriteLine($"\n[DSL] Парсинг и запуск скрипта: {script}");
            systemFacade.ExecuteDsl(script);

            Console.WriteLine($"[Результат DSL] Координаты кнопки: X={dangerButton.BoundingBox.X}, Y={dangerButton.BoundingBox.Y}");

            // 7. Проверка Undo (Command)
            Console.WriteLine("\n[Undo] Отмена последних изменений...");
            cmdManager.Undo();
            Console.WriteLine($"[Результат Undo] Координаты вернулись к: X={dangerButton.BoundingBox.X}, Y={dangerButton.BoundingBox.Y}");

            Console.WriteLine("\n=== СБОРКА И РЕБИЛД СИСТЕМЫ ПОЛНОСТЬЮ ЗАВЕРШЕНЫ ===");
        }
    }
}