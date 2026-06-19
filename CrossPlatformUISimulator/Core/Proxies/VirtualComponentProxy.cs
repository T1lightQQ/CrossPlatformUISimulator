using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Infrastructure;
using CrossPlatformUISimulator.Core.Components;

namespace CrossPlatformUISimulator.Core.Proxies
{
    // Если этого контракта не было в папке Abstractions, объявляем его здесь:
    public interface ILazyComponentProxy : IUIComponent
    {
        bool IsMaterialized { get; }
        void Materialize();
        IUIComponent GetRealSubject();
    }

    public class VirtualComponentProxy : ILazyComponentProxy
    {
        private IUIComponent? _realSubject;

        public string Id { get; }
        public bool IsMaterialized => _realSubject != null;
        public Rectangle BoundingBox { get; set; }
        public string TextContent { get; set; } = "";
        public bool Enabled { get; set; } = true;

        // Свойство Flyweight заставляет прокси материализовать реальный объект
        public IUIStyleFlyweight Flyweight
        {
            get => GetRealSubject().Flyweight;
            set => GetRealSubject().Flyweight = value;
        }

        public VirtualComponentProxy(string id, Rectangle bounds)
        {
            Id = id;
            BoundingBox = bounds;
        }

        public void Materialize()
        {
            if (_realSubject == null)
            {
                // Ленивое создание тяжелого объекта "по требованию"
                var defaultKey = new StyleKey("Arial", 12, 0, 0, 0);
                _realSubject = new ButtonComponent(Id, BoundingBox, FlyweightFactory.Instance.GetFlyweight(defaultKey))
                {
                    TextContent = this.TextContent,
                    Enabled = this.Enabled
                };
            }
        }

        public IUIComponent GetRealSubject()
        {
            Materialize();
            return _realSubject!;
        }

        public void Render(IRenderingContext ctx) => GetRealSubject().Render(ctx);
        public void SetPosition(Point pos) => BoundingBox = new Rectangle(pos.X, pos.Y, BoundingBox.Width, BoundingBox.Height);
        public void SetMediator(IUIComponentMediator mediator) => GetRealSubject().SetMediator(mediator);
        public T? FindById<T>(string id) where T : class, IUIComponent => Id == id ? this as T : _realSubject?.FindById<T>(id);

        public IUIComponent Clone() => new VirtualComponentProxy(Id, BoundingBox);

        public void Dispose() => _realSubject?.Dispose();
    }
}