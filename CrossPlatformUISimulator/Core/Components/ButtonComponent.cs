using System;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Core.Components
{
    public class ButtonComponent : UIComponentBase
    {
        public ButtonComponent(string id, Rectangle bounds, IUIStyleFlyweight flyweight)
            : base(id, bounds, flyweight) { }

        
        public override void Render(IRenderingContext ctx)
        {
            
            Console.WriteLine($"[Render] Кнопка {Id} отрисована в состоянии: {CurrentState.StateName}");
        }

        
        public void SimulateClick()
        {
            Notify(new UIStateChangeData("UserClick", "NormalState", "Clicked", DateTime.UtcNow));
        }

        // Прототип
        // Не глубокое копирование
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