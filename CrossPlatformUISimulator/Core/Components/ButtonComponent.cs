using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Core.Components
{
    public class ButtonComponent : UIComponentBase
    {
        public ButtonComponent(string id, Rectangle bounds, IUIStyleFlyweight flyweight)
            : base(id, bounds, flyweight) { }

        public override void Render(IRenderingContext ctx)
        {
            // Логика отрисовки кнопки (в симуляторе оставляем пустой или пишем лог)
        }

        public override IUIComponent Clone() =>
            new ButtonComponent(Id, BoundingBox, Flyweight) { Enabled = Enabled, TextContent = TextContent };

        // Инкапсуляция действия: Кнопка ничего не знает о других окнах, она просто шлет сигнал Медиатору
        public void SimulateClick()
        {
            Mediator?.Notify(this, new UIEvent(Guid.NewGuid(), DateTime.UtcNow, "UI_Click", Id));
        }
    }
}