using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Behavioral.State;

namespace CrossPlatformUISimulator.Behavioral.Command
{
    public class StartLoadingCommand : IUICommand
    {
        private readonly IUIComponent _component;
        private IComponentState? _previousState;

        public string Description => "Перевод компонента в режим загрузки";

        public StartLoadingCommand(IUIComponent component) => _component = component;

        public void Execute()
        {
            _previousState = _component.CurrentState;
            _component.TransitionTo(LoadingState.Instance);
        }

        public void Undo()
        {
            if (_previousState != null)
            {
                _component.TransitionTo(_previousState);
            }
        }
    }

    public class TriggerErrorCommand : IUICommand
    {
        private readonly IUIComponent _component;
        private IComponentState? _previousState;

        public string Description => "Перевод компонента в аварийный режим";

        public TriggerErrorCommand(IUIComponent component) => _component = component;

        public void Execute()
        {
            _previousState = _component.CurrentState;
            _component.TransitionTo(ErrorState.Instance);
        }

        public void Undo()
        {
            if (_previousState != null)
            {
                _component.TransitionTo(_previousState);
            }
        }
    }
}