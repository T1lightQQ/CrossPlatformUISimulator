using System;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Core.Components
{
    public class ButtonComponent : UIComponentBase
    {
        public ButtonComponent(string id, Rectangle bounds, IUIStyleFlyweight flyweight)
            : base(id, bounds, flyweight) { }

        // ИСПРАВЛЕНО: Безопасный рендеринг без привязки к недописанному интерфейсу состояний
        public override void Render(IRenderingContext ctx)
        {
            // Здесь логика отрисовки кнопки в зависимости от её имени состояния
            Console.WriteLine($"[Render] Кнопка {Id} отрисована в состоянии: {CurrentState.StateName}");
        }

        // ИСПРАВЛЕНО: Добавлен метод симуляции клика для ComponentStates.cs
        public void SimulateClick()
        {
            Notify(new UIStateChangeData("UserClick", "NormalState", "Clicked", DateTime.UtcNow));
        }

        public override IUIComponent Clone()
        {
            return new ButtonComponent(Id, BoundingBox, Flyweight) { Enabled = Enabled, TextContent = TextContent };
        }

        public override void Accept(IUIComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}