using System.Collections.Generic;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.DSL
{
    // Синтаксический анализатор (Парсер грамматики)
    public class ScriptParser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;

        public ScriptParser(List<Token> tokens) => _tokens = tokens;

        public IExpression Parse()
        {
            if (_tokens[_current].Type == TokenType.Select)
            {
                _current++;
                var selector = new TypeSelectorExpression(_tokens[_current++].Value);
                WhereExpression? where = null;

                if (_tokens[_current].Type == TokenType.Where)
                {
                    _current++;
                    where = new WhereExpression(new PredicateExpression(_tokens[_current++].Value));
                }

                if (_tokens[_current].Type == TokenType.Arrow)
                {
                    _current++; // пропуск ->
                    _current++; // пропуск EXECUTE
                    return new PipelineExpression(new SelectExpression(selector, where), ParseExecute());
                }
                return new SelectExpression(selector, where);
            }

            if (_tokens[_current].Type == TokenType.Execute)
            {
                _current++;
                return ParseExecute();
            }

            throw new ParseException("Неизвестное начало выражения", _current);
        }

        private ExecuteExpression ParseExecute()
        {
            var actions = new List<ActionExpression> { new ActionExpression(_tokens[_current++].Value) };
            while (_tokens[_current].Type == TokenType.Arrow)
            {
                _current++;
                actions.Add(new ActionExpression(_tokens[_current++].Value));
            }
            return new ExecuteExpression(new ChainExpression(actions));
        }
    }
}