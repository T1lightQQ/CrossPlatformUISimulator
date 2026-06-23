using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Core.Decorators
{
    public class BorderDecorator : UIComponentDecorator
    {
        // Декоратор
        public BorderDecorator(IUIComponent component) : base(component) { }

        public override void Render(IRenderingContext ctx)
        {
            
            base.Render(ctx);
        }

        public override IUIComponent Clone()
        {
            
            return new BorderDecorator(WrappedComponent.Clone());
        }
    }
}