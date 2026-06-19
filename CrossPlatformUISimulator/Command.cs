using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    // --- РЕАЛИЗАЦИЯ КОМАНД С DELTA-BASED UNDO ---

    public class MoveComponentCommand : IUICommand
    {
        private readonly IUIComponent _component;
        private readonly Point _newPosition;
        private Point? _oldPosition;

        public string Description => $"Перемещение {_component.Id} в ({_newPosition.X}, {_newPosition.Y})";

        public MoveComponentCommand(IUIComponent component, Point newPosition)
        {
            _component = component;
            _newPosition = newPosition;
        }

        public void Execute()
        {
            _oldPosition = new Point(_component.BoundingBox.X, _component.BoundingBox.Y);
            _component.SetPosition(_newPosition);
        }

        public void Undo()
        {
            if (_oldPosition != null)
            {
                _component.SetPosition(_oldPosition);
            }
        }
    }

    public class ApplyThemeCommand : IUICommand
    {
        private readonly IUIComponent _component;
        private readonly StyleKey _newStyle;
        private StyleKey _oldStyle;

        public string Description => $"Смена стиля у {_component.Id}";

        public ApplyThemeCommand(IUIComponent component, StyleKey newStyle)
        {
            _component = component;
            _newStyle = newStyle;
        }

        public void Execute()
        {
            if (_component is UIComponentBase baseComp)
            {
                _oldStyle = new StyleKey(baseComp.Flyweight.Font.Name, baseComp.Flyweight.Font.Size, baseComp.Flyweight.Palette.Background.R, baseComp.Flyweight.Palette.Background.G, baseComp.Flyweight.Palette.Background.B);
                baseComp.Flyweight = FlyweightFactory.Instance.GetFlyweight(_newStyle);
            }
        }

        public void Undo()
        {
            if (_component is UIComponentBase baseComp)
            {
                baseComp.Flyweight = FlyweightFactory.Instance.GetFlyweight(_oldStyle);
            }
        }
    }

    public class ToggleDecoratorCommand : IUICommand
    {
        private readonly IContainerComponent _parent;
        private readonly string _targetId;
        private readonly DecoratorType _decoratorType;
        private IUIComponent? _originalComponent;
        private IUIComponent? _decoratedComponent;
        private bool _isApplied = false;

        public string Description => $"Переключение декоратора {_decoratorType} на {_targetId}";

        public ToggleDecoratorCommand(IContainerComponent parent, string targetId, DecoratorType decoratorType)
        {
            _parent = parent;
            _targetId = targetId;
            _decoratorType = decoratorType;
        }

        public void Execute()
        {
            if (!_isApplied)
            {
                _originalComponent = _parent.FindById<IUIComponent>(_targetId);
                if (_originalComponent == null) return;

                _decoratedComponent = _decoratorType switch
                {
                    DecoratorType.Border => new BorderDecorator(_originalComponent),
                    DecoratorType.RenderLog => new RenderLogDecorator(_originalComponent),
                    _ => new CachedRenderDecorator(_originalComponent)
                };

                _parent.ReplaceChild(_targetId, _decoratedComponent);
                _isApplied = true;
            }
            else
            {
                Undo();
            }
        }

        public void Undo()
        {
            if (_isApplied && _originalComponent != null)
            {
                _parent.ReplaceChild(_targetId, _originalComponent);
                _isApplied = false;
            }
        }
    }

    public class MacroCommand : IUICommand
    {
        private readonly IUICommand[] _commands;
        private readonly List<IUICommand> _executedCommands = new();

        public string Description => $"Макро-команда из {_commands.Length} операций.";

        public MacroCommand(IUICommand[] commands) => _commands = commands;

        public void Execute()
        {
            _executedCommands.Clear();
            try
            {
                foreach (var cmd in _commands)
                {
                    cmd.Execute();
                    _executedCommands.Add(cmd);
                }
            }
            catch (Exception)
            {
                // Стратегия Rollback: отмена уже выполненных частей при падении промежуточной команды
                for (int i = _executedCommands.Count - 1; i >= 0; i--)
                {
                    _executedCommands[i].Undo();
                }
                throw;
            }
        }

        public void Undo()
        {
            for (int i = _commands.Length - 1; i >= 0; i--)
            {
                _commands[i].Undo();
            }
        }
    }

    // --- МЕНЕДЖЕР КОМАНД (INVOKER) ---
    public class CommandManager
    {
        private readonly Stack<IUICommand> _history = new();
        private readonly Stack<IUICommand> _redoStack = new();
        private readonly int _maxHistorySize;
        private readonly object _lock = new();

        public CommandManager(int maxHistorySize = 1000) => _maxHistorySize = maxHistorySize;

        public void Execute(IUICommand command)
        {
            lock (_lock)
            {
                try
                {
                    command.Execute();

                    if (_history.Count >= _maxHistorySize)
                    {
                        _history.Pop(); // Вытесняем старую историю при лимите
                        ApplicationTelemetrySingleton.Instance.LogOperation("Command", "HistoryEvicted", TimeSpan.Zero);
                    }

                    _history.Push(command);
                    _redoStack.Clear(); // Новая команда очищает Redo

                    ApplicationTelemetrySingleton.Instance.LogOperation("Command", "Execute", TimeSpan.Zero, command.Description);
                }
                catch (InvalidOperationException ex)
                {
                    ApplicationTelemetrySingleton.Instance.LogOperation("Command", "RejectedByProxy", TimeSpan.Zero, ex.Message);
                    throw;
                }
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
                    _redoStack.Push(cmd);
                    ApplicationTelemetrySingleton.Instance.LogOperation("Command", "Undo", TimeSpan.Zero, cmd.Description);
                }
            }
        }

        public void Redo()
        {
            lock (_lock)
            {
                if (_redoStack.Count > 0)
                {
                    var cmd = _redoStack.Pop();
                    cmd.Execute();
                    _history.Push(cmd);
                    ApplicationTelemetrySingleton.Instance.LogOperation("Command", "Redo", TimeSpan.Zero, cmd.Description);
                }
            }
        }

        public void ClearHistory()
        {
            lock (_lock)
            {
                _history.Clear();
                _redoStack.Clear();
            }
        }
    }
}