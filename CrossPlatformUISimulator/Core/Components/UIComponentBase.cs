using System;
using System.Collections.Generic;
using System.Threading;
using ExtrinsicComponentState = CrossPlatformUISimulator.Common.ExtrinsicComponentState;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Infrastructure;
using CrossPlatformUISimulator.Behavioral.Memento;
using CrossPlatformUISimulator.Behavioral.Observer;
using CrossPlatformUISimulator.Behavioral.State;

namespace CrossPlatformUISimulator.Core.Components
{
    public abstract class UIComponentBase : IUIComponent, IOriginator
    {
        
        public abstract void Accept(IUIComponentVisitor visitor);

        protected IUIComponentMediator? Mediator;
        private readonly SafeObserverSubject _stateSubject;
        private IComponentState _currentState = NormalState.Instance;
        private readonly object _stateLock = new();

        public string Id { get; }
        public Rectangle BoundingBox { get; set; }
        public string TextContent { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public IUIStyleFlyweight Flyweight { get; set; }

        public IComponentState CurrentState => Volatile.Read(ref _currentState);

        protected UIComponentBase(string id, Rectangle bounds, IUIStyleFlyweight flyweight)
        {
            Id = id;
            BoundingBox = bounds;
            Flyweight = flyweight;
            _stateSubject = new SafeObserverSubject(this);
        }

        public void SetMediator(IUIComponentMediator mediator) => Mediator = mediator;

        public abstract void Render(IRenderingContext ctx);

        public void SetPosition(Point pos) =>
            BoundingBox = new Rectangle(pos.X, pos.Y, BoundingBox.Width, BoundingBox.Height);

        public virtual T? FindById<T>(string id) where T : class, IUIComponent
        {
            if (Id == id && this is T target) return target;
            return null;
        }

        public abstract IUIComponent Clone();

        #region Реализация паттернов State и Observer (Subject)

        public void TransitionTo(IComponentState newState)
        {
            if (newState == null) return;
            IComponentState oldState;

            lock (_stateLock)
            {
                if (_currentState.StateName == newState.StateName) return;

                oldState = _currentState;
                oldState.Exit(this);

                _currentState = newState; // Смена состояния под атомарной блокировкой
                _currentState.Enter(this);
            }

            // Нотификация наблюдателей вынесена за пределы lock во избежание взаимных блокировок (Deadlocks)
            _stateSubject.Notify(new UIStateChangeData("ComponentState", oldState.StateName, newState.StateName, DateTime.UtcNow));
        }

        public void Attach(IUIStateObserver observer) => _stateSubject.Attach(observer);
        public void Detach(IUIStateObserver observer) => _stateSubject.Detach(observer);
        public void Notify(UIStateChangeData data) => _stateSubject.Notify(data);

        #endregion

        #region Реализация паттерна Memento (Originator)

        public virtual IMemento CreateMemento()
        {
            var styleImpl = (UIStyleFlyweightImpl)Flyweight;

            var states = new Dictionary<string, ExtrinsicComponentState>
            {
                { Id, new ExtrinsicComponentState(BoundingBox, TextContent, Enabled, styleImpl.Key, CurrentState.StateName) }
            };
            return new TreeConfigurationMemento(states);
        }

        public virtual void Restore(IMemento memento)
        {
            if (memento is TreeConfigurationMemento treeMemento)
            {
                if (!treeMemento.SnapshotStates.TryGetValue(Id, out var state))
                    throw new MementoIncompatibleException($"Компонент с ID '{Id}' отсутствует в восстанавливаемом снапшоте.");

                BoundingBox = state.Bounds;
                TextContent = state.Text;
                Enabled = state.Enabled;
                Flyweight = FlyweightFactory.Instance.GetFlyweight(state.Style);

                // Восстановление полиморфного состояния на базе сохраненного имени
                IComponentState TargetState = state.StateName switch
                {
                    "NormalState" => NormalState.Instance,
                    "LoadingState" => LoadingState.Instance,
                    "ErrorState" => ErrorState.Instance,
                    _ => NormalState.Instance
                };

                TransitionTo(TargetState);
            }
        }


        #endregion

        public virtual void Dispose()
        {
            Mediator?.Unregister(this);
            _stateSubject.ClearAll(); // Автоматический разрыв связей при уничтожении
        }
    }
}