using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Behavioral.Command
{
    public class CommandManager
    {
        private readonly Stack<IUICommand> _history = new();

        public void Execute(IUICommand command)
        {
            command.Execute();
            _history.Push(command);
        }

        public void Undo()
        {
            if (_history.Count > 0)
            {
                var cmd = _history.Pop();
                cmd.Undo();
            }
        }
    }
}