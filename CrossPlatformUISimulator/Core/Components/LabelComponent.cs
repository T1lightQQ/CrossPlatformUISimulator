using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Core.Components
{
    public class LabelComponent : UIComponentBase
    {
        public LabelComponent(string id, Rectangle bounds, IUIStyleFlyweight flyweight)
            : base(id, bounds, flyweight) { }

        public override void Render(IRenderingContext ctx)
        {
            // Логика отрисовки текстовой метки (в симуляторе оставляем пустой)
        }

        public override IUIComponent Clone() =>
            new LabelComponent(Id, BoundingBox, Flyweight)
            {
                Enabled = Enabled,
                TextContent = TextContent
            };
    }
}