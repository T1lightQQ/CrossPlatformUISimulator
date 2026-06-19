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
            // Делегирование рендеринга объекту состояния
            CurrentState.HandleRender(this, ctx);
        }

        public override IUIComponent Clone() =>
            new ButtonComponent(Id, BoundingBox, Flyweight) { Enabled = Enabled, TextContent = TextContent };

        public void SimulateClick()
        {
            Mediator?.Notify(this, new UIEvent(Guid.NewGuid(), DateTime.UtcNow, "UI_Click", Id));
        }

        // Пользовательское действие теперь полностью проксируется через паттерн State
        public void UserClick()
        {
            CurrentState.HandleClick(this);
        }
    }
}