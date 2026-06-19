using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Behavioral.Command
{
    
    public class MoveComponentCommand : IUICommand
    {
        private readonly IUIComponent _component;
        private readonly Point _newPos;
        private Point _oldPos;

        public string Description => "Перемещение компонента";

        public MoveComponentCommand(IUIComponent component, Point newPos)
        {
            _component = component;
            _newPos = newPos;
        }

        public void Execute()
        {
            _oldPos = new Point(_component.BoundingBox.X, _component.BoundingBox.Y);
            _component.SetPosition(_newPos);
        }

        public void Undo() => _component.SetPosition(_oldPos);
    }

    
    public class ApplyThemeCommand : IUICommand
    {
        private readonly IUIComponent _component;
        private readonly StyleKey _newStyle;
        private IUIStyleFlyweight? _oldStyle;

        public string Description => "Изменение стиля";

        public ApplyThemeCommand(IUIComponent component, StyleKey newStyle)
        {
            _component = component;
            _newStyle = newStyle;
        }

        public void Execute()
        {
            _oldStyle = _component.Flyweight;
            
            _component.Flyweight = Infrastructure.FlyweightFactory.Instance.GetFlyweight(_newStyle);
        }

        public void Undo()
        {
            if (_oldStyle != null) _component.Flyweight = _oldStyle;
        }
    }

    
    public class MacroCommand : IUICommand
    {
        private readonly IUICommand[] _commands;
        public string Description => "Групповая операция";

        public MacroCommand(IUICommand[] commands) => _commands = commands;

        public void Execute()
        {
            foreach (var cmd in _commands) cmd.Execute();
        }

        public void Undo()
        {
            for (int i = _commands.Length - 1; i >= 0; i--)
            {
                _commands[i].Undo();
            }
        }
    }
}