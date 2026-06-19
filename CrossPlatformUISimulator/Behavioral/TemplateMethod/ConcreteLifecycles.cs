using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Core.Proxies;
using System;

namespace CrossPlatformUISimulator.Behavioral.TemplateMethod
{
    // Жизненный цикл стандартного одиночного элемента UI
    public class StandardComponentLifecycle : UIComponentLifecycleBase
    {
        public StandardComponentLifecycle(IUIComponent component) : base(component) { }

        protected override void Initialize(UIContext ctx)
        {
            // Базовая инициализация геометрии
        }

        protected override bool Validate(UIContext ctx)
        {
            // Компонент должен быть доступен для отрисовки
            return TargetComponent.CurrentState.StateName != "ErrorState";
        }

        protected override void Render(UIContext ctx)
        {
            // Вызов полиморфного рендеринга состояния
            if (TargetComponent.CurrentState.StateName == "NormalState")
            {
                // Отрисовка
            }
        }
    }

    // Сложный жизненный цикл контейнеров
    public class ComplexContainerLifecycle : UIComponentLifecycleBase
    {
        private readonly IContainerComponent _container;

        public ComplexContainerLifecycle(IContainerComponent container) : base(container)
        {
            _container = container;
        }

        protected override void Initialize(UIContext ctx)
        {
            // Первичная сонастройка дочерних элементов
        }

        protected override void LoadResources(UIContext ctx)
        {
            // Переопределенный шаг: тяжелая загрузка ресурсов всех дочерних узлов
            foreach (var child in _container.Children)
            {
                if (child is ILazyComponentProxy proxy && !proxy.IsMaterialized)
                {
                    // Провоцируем ленивую сборку прокси при необходимости
                    var text = child.TextContent;
                }
            }
        }

        protected override bool Validate(UIContext ctx)
        {
            // Валидация каскадом по всему дереву Composite
            if (_container.CurrentState.StateName == "ErrorState") return false;

            foreach (var child in _container.Children)
            {
                if (child.CurrentState.StateName == "ErrorState") return false;
            }
            return true;
        }

        protected override void Render(UIContext ctx)
        {
            // Рендеринг контейнера делегирует выполнение текущей LayoutStrategy
            var layoutContext = new LayoutContext(10, 5, 800, 600, 1.0);
            _container.CalculateLayout(layoutContext);
        }
    }
}