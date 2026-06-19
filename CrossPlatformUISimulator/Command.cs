using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class MoveComponentCommand : IUICommand
    {
        private readonly IUIComponent _comp;
        private readonly Point _newPos;
        private Point _oldPos;

        public string Description => $"Перемещение {_comp.Id}";

        public MoveComponentCommand(IUIComponent comp, Point newPos) { _comp = comp; _newPos = newPos; }

        public void Execute()
        {
            _oldPos = new Point(_comp.BoundingBox.X, _comp.BoundingBox.Y);
            _comp.SetPosition(_newPos);
        }

        public void Undo() => _comp.SetPosition(_oldPos);
    }

    public class ApplyThemeCommand : IUICommand
    {
        private readonly IUIComponent _comp;
        private readonly StyleKey _style;
        private IUIStyleFlyweight _oldStyle;

        public string Description => $"Смена стиля {_comp.Id}";

        public ApplyThemeCommand(IUIComponent comp, StyleKey style) { _comp = comp; _style = style; }

        public void Execute()
        {
            if (_comp is UIComponentBase b)
            {
                _oldStyle = b.Flyweight;
                b.Flyweight = FlyweightFactory.Instance.GetFlyweight(_style);
            }
        }

        public void Undo() { if (_comp is UIComponentBase b) b.Flyweight = _oldStyle; }
    }

    public class MacroCommand : IUICommand
    {
        private readonly IUICommand[] _cmds;
        public string Description => "Атомарный пакет команд.";

        public MacroCommand(IUICommand[] cmds) => _cmds = cmds;

        public void Execute()
        {
            foreach (var cmd in _cmds) cmd.Execute();
        }

        public void Undo()
        {
            for (int i = _cmds.Length - 1; i >= 0; i--) _cmds[i].Undo();
        }
    }

    public class CommandManager
    {
        private readonly Stack<IUICommand> _history = new();
        private readonly Stack<IUICommand> _redo = new();
        private readonly object _lock = new();

        public void Execute(IUICommand cmd)
        {
            lock (_lock)
            {
                cmd.Execute();
                _history.Push(cmd);
                _redo.Clear();
            }
        }

        public void Undo()
        {
            lock (_lock)
            {
                if (_history.Count > 0)
                {
                    var cmd = _history.Pop();
                    cmd.Undo();
                    _redo.Push(cmd);
                }
            }
        }
    }
}