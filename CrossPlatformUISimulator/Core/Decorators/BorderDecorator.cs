using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Core.Decorators
{
    public class BorderDecorator : UIComponentDecorator
    {
        public BorderDecorator(IUIComponent component) : base(component) { }

        public override void Render(IRenderingContext ctx)
        {
            // 1. Отрисовка рамки вокруг компонента
            // 2. Вызов базовой отрисовки самого компонента
            base.Render(ctx);
        }

        public override IUIComponent Clone() => new BorderDecorator(Component.Clone());
    }
}