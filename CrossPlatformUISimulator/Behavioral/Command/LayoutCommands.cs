using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Behavioral.Command
{
    public class ApplyLayoutCommand : IUICommand
    {
        private readonly IContainerComponent _container;
        private readonly ILayoutStrategy _newStrategy;
        private readonly LayoutContext _context;
        private ILayoutStrategy? _previousStrategy;
        private IMemento? _backupState;

        public string Description => $"Применение стратегии разметки {_newStrategy.GetType().Name}";

        public ApplyLayoutCommand(IContainerComponent container, ILayoutStrategy newStrategy, LayoutContext context)
        {
            _container = container;
            _newStrategy = newStrategy;
            _context = context;
        }

        public void Execute()
        {
            
            if (_container is IOriginator originator)
            {
                _backupState = originator.CreateMemento();
            }

            
            var calculatedBounds = _container.CalculateLayout(_context);

            
            foreach (var child in _container.Children)
            {
                if (calculatedBounds.TryGetValue(child.Id, out var newBounds))
                {
                    child.BoundingBox = newBounds;
                }
            }

            
            _container.SetLayoutStrategy(_newStrategy);

            
            _container.Notify(new UIStateChangeData("LayoutRecalculated", "OldLayout", _newStrategy.GetType().Name, System.DateTime.UtcNow));
        }

        public void Undo()
        {
            if (_backupState != null && _container is IOriginator originator)
            {
                originator.Restore(_backupState);
            }
        }
    }
}