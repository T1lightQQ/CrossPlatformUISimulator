using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Core.Decorators
{
    public class BorderDecorator : UIComponentDecorator
    {
        public BorderDecorator(IUIComponent component) : base(component) { }

        public override void Render(IRenderingContext ctx)
        {
            // Симуляция логики отрисовки декоративной рамки вокруг компонента
            base.Render(ctx);
        }

        public override IUIComponent Clone()
        {
            // Исправлено: использование WrappedComponent из базового класса UIComponentDecorator
            return new BorderDecorator(WrappedComponent.Clone());
        }
    }
}