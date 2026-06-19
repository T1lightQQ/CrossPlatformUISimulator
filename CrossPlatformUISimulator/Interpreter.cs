using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    // Контекст выполнения интерпретатора UI-DSL
    public class UIInterpreterContext
    {
        public IUISystemFacade Facade { get; }
        public CommandManager CommandManager { get; }
        public IApplicationTelemetry Telemetry { get; }
        public List<IUIComponent> SelectedComponents { get; set; } = new();

        public UIInterpreterContext(IUISystemFacade facade, CommandManager commandManager, IApplicationTelemetry telemetry)
        {
            Facade = facade;
            CommandManager = commandManager;
            Telemetry = telemetry;
        }
    }

    // --- РЕАЛИЗАЦИЯ AST-НОД (PATTERN INTERPRETER) ---

    public readonly record struct TypeSelectorExpression(string Selector) : IExpression
    {
        public void Interpret(UIInterpreterContext context) { }
    }

    public readonly record struct PredicateExpression(string Predicate) : IExpression
    {
        public void Interpret(UIInterpreterContext context) { }
    }

    public readonly record struct ActionExpression(string ActionText) : IExpression
    {
        public void Interpret(UIInterpreterContext context) { }
    }

    // Non-Terminal: Выражение выборки элементов через фабрику итераторов
    public record SelectExpression(TypeSelectorExpression Selector, WhereExpression? Where) : IExpression
    {
        public void Interpret(UIInterpreterContext context)
        {
            var factory = new IteratorFactory();
            var iterator = factory.CreateDfs(context.Facade.RootTree);
            var results = new List<IUIComponent>();

            string targetType = Selector.Selector;

            while (iterator.MoveNext())
            {
                var comp = (IUIComponent)iterator.Current;
                bool isMatch = targetType == "<*>" ||
                              (targetType == "<Button>" && comp is ButtonComponent) ||
                              (targetType == "<Label>" && comp is LabelComponent);

                if (isMatch)
                {
                    results.Add(comp);
                }
            }

            context.SelectedComponents = results;
            Where?.Interpret(context);
        }
    }

    // Non-Terminal: Фильтрация выбранного подмножества по предикатам
    public record WhereExpression(PredicateExpression PredicateExpr) : IExpression
    {
        public void Interpret(UIInterpreterContext context)
        {
            var filtered = new List<IUIComponent>();
            string condition = PredicateExpr.Predicate;

            foreach (var comp in context.SelectedComponents)
            {
                bool isValid = false;
                if (condition == "Visible=true") isValid = true;
                else if (condition == "Enabled=false") isValid = !comp.Enabled;
                else if (condition.StartsWith("Id='") && condition.EndsWith("'"))
                {
                    string id = condition.Substring(4, condition.Length - 5);
                    isValid = (comp.Id == id);
                }

                if (isValid) filtered.Add(comp);
            }

            context.SelectedComponents = filtered;
        }
    }

    // Non-Terminal: Парсинг и генерация макро-цепочки команд для изменения состояния компонентов
    public record ExecuteExpression(ChainExpression Chain) : IExpression
    {
        public void Interpret(UIInterpreterContext context) => Chain.Interpret(context);
    }

    public record ChainExpression(List<ActionExpression> Actions) : IExpression
    {
        public void Interpret(UIInterpreterContext context)
        {
            var aggregateCommands = new List<IUICommand>();

            foreach (var comp in context.SelectedComponents)
            {
                var singleComponentCommands = new List<IUICommand>();

                foreach (var action in Actions)
                {
                    string token = action.ActionText;
                    if (token.StartsWith("ApplyTheme("))
                    {
                        singleComponentCommands.Add(new ApplyThemeCommand(comp, new StyleKey("Calibri", 14, 30, 30, 30)));
                    }
                    else if (token.StartsWith("SetPosition("))
                    {
                        singleComponentCommands.Add(new MoveComponentCommand(comp, new Point(10, 20)));
                    }
                }

                if (singleComponentCommands.Count > 0)
                {
                    aggregateCommands.Add(new MacroCommand(singleComponentCommands.ToArray()));
                }
            }

            if (aggregateCommands.Count > 0)
            {
                var executionMacro = new MacroCommand(aggregateCommands.ToArray());
                context.CommandManager.Execute(executionMacro);
            }
        }
    }

    // Гибридный конвейерный узел для поддержки синтаксиса "SELECT ... WHERE ... -> EXECUTE ..."
    public record PipelineExpression(SelectExpression Select, ExecuteExpression Execute) : IExpression
    {
        public void Interpret(UIInterpreterContext context)
        {
            Select.Interpret(context);
            Execute.Interpret(context);
        }
    }

    // Исключение этапа синтаксического анализа грамматики DSL
    public class ParseException : Exception
    {
        public int Position { get; }
        public ParseException(string message, int position)
            : base($"Ошибка компиляции DSL: {message} на позиции {position}") => Position = position;
    }

    // --- СТРОГИЙ РЕКУРСИВНЫЙ НИСХОДЯЩИЙ ПАРСЕР (RECURSIVE DESCENT PARSER) ---
    public class ScriptParser
    {
        private readonly List<Token> _tokens;
        private int _cursor = 0;

        public ScriptParser(List<Token> tokens) => _tokens = tokens;

        private Token Peek() => _tokens[_cursor];
        private Token Advance() { if (!IsAtEnd()) _cursor++; return _tokens[_cursor - 1]; }
        private bool IsAtEnd() => Peek().Type == TokenType.EOF;
        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

        private bool Match(TokenType type)
        {
            if (Check(type)) { Advance(); return true; }
            return false;
        }

        public IExpression ParseExpressionTree()
        {
            if (Match(TokenType.Select))
            {
                if (!Check(TokenType.Selector)) throw new ParseException("Ожидался селектор типа (<*>, <Button>).", Peek().Position);
                var selector = new TypeSelectorExpression(Advance().Value);

                WhereExpression? whereNode = null;
                if (Match(TokenType.Where))
                {
                    if (!Check(TokenType.Predicate)) throw new ParseException("Ожидалось выражение предиката.", Peek().Position);
                    whereNode = new WhereExpression(new PredicateExpression(Advance().Value));
                }

                if (Match(TokenType.Arrow))
                {
                    if (!Match(TokenType.Execute)) throw new ParseException("Ожидался терм EXECUTE после оператора конвейера '->'.", Peek().Position);
                    var executeNode = ParseExecuteChain();
                    return new PipelineExpression(new SelectExpression(selector, whereNode), executeNode);
                }

                return new SelectExpression(selector, whereNode);
            }

            if (Match(TokenType.Execute))
            {
                return ParseExecuteChain();
            }

            throw new ParseException("Точка входа скрипта должна начинаться с SELECT или EXECUTE.", Peek().Position);
        }

        private ExecuteExpression ParseExecuteChain()
        {
            var actions = new List<ActionExpression>();
            if (!Check(TokenType.Action)) throw new ParseException("После директивы EXECUTE должно следовать атомарное действие.", Peek().Position);

            actions.Add(new ActionExpression(Advance().Value));

            while (Match(TokenType.Arrow))
            {
                if (!Check(TokenType.Action)) throw new ParseException("Ожидалось описание действия после символа конвейера.", Peek().Position);
                actions.Add(new ActionExpression(Advance().Value));
            }

            return new ExecuteExpression(new ChainExpression(actions));
        }
    }
}
