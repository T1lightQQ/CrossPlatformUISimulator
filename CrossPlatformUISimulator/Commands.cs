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

    // Команда сохранения чекпоинта, инкапсулирующая вызовы Caretaker
    public class SaveCheckpointCommand : IUICommand
    {
        private readonly UIMementoManager _manager;
        private readonly IOriginator _root;
        private readonly string _label;

        public string Description => $"Создание снимка состояния {_label}";

        public SaveCheckpointCommand(UIMementoManager manager, IOriginator root, string label)
        {
            _manager = manager;
            _root = root;
            _label = label;
        }

        public void Execute() => _manager.SaveCheckpoint(_label, _root);
        public void Undo() => TelemetrySingleton.Instance.LogEvent("Undo", "Снапшоты сессии не подлежат инкрементальной отмене.");
    }

    public class CommandManager
    {
        private readonly System.Collections.Generic.Stack<IUICommand> _history = new();

        public void Execute(IUICommand cmd)
        {
            cmd.Execute();
            _history.Push(cmd);
        }

        public void Undo()
        {
            if (_history.Count > 0) _history.Pop().Undo();
        }
    }
}