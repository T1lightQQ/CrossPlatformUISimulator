using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Core.Proxies
{
    public interface ILazyComponentProxy
    {
        bool IsMaterialized { get; }
    }

    public class VirtualComponentProxy : IUIComponent, ILazyComponentProxy
    {
        private readonly Func<IUIComponent> _factory;
        private IUIComponent? _instance;

        public string Id { get; }
        public bool IsMaterialized => _instance != null;

        public VirtualComponentProxy(string id, Func<IUIComponent> factory)
        {
            Id = id;
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        private IUIComponent EnsureMaterialized()
        {
            if (_instance == null)
            {
                _instance = _factory();
            }
            return _instance;
        }

        public Rectangle BoundingBox
        {
            get => EnsureMaterialized().BoundingBox;
            set => EnsureMaterialized().BoundingBox = value;
        }

        public string TextContent
        {
            get => EnsureMaterialized().TextContent;
            set => EnsureMaterialized().TextContent = value;
        }

        public bool Enabled
        {
            get => EnsureMaterialized().Enabled;
            set => EnsureMaterialized().Enabled = value;
        }

        public IUIStyleFlyweight Flyweight
        {
            get => EnsureMaterialized().Flyweight;
            set => EnsureMaterialized().Flyweight = value;
        }

        // ЧАСТЬ 10: Проксирование вызовов состояния к реальному материализованному объекту
        public IComponentState CurrentState => EnsureMaterialized().CurrentState;
        public void TransitionTo(IComponentState newState) => EnsureMaterialized().TransitionTo(newState);
        public void Attach(IUIStateObserver observer) => EnsureMaterialized().Attach(observer);
        public void Detach(IUIStateObserver observer) => EnsureMaterialized().Detach(observer);
        public void Notify(UIStateChangeData data) => EnsureMaterialized().Notify(data);

        public void Render(IRenderingContext ctx) => EnsureMaterialized().Render(ctx);
        public void SetPosition(Point position) => EnsureMaterialized().SetPosition(position);
        public void SetMediator(IUIComponentMediator mediator) => EnsureMaterialized().SetMediator(mediator);

        public T? FindById<T>(string id) where T : class, IUIComponent
        {
            if (Id == id) return EnsureMaterialized() as T;
            return EnsureMaterialized().FindById<T>(id);
        }

        public IUIComponent Clone() => new VirtualComponentProxy(Id, _factory);

        public void Dispose() => _instance?.Dispose();
    }
}