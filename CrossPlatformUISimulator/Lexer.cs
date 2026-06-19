using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    // Потокобезопасный лексический анализатор (Сканер токенов)
    public class Scanner
    {
        private readonly string _src;
        private int _index = 0;

        public Scanner(string source) => _src = source;

        public List<Token> ScanTokens()
        {
            var tokens = new List<Token>();

            while (_index < _src.Length)
            {
                char current = _src[_index];
                if (char.IsWhiteSpace(current)) { _index++; continue; }

                if (_src.AsSpan(_index).StartsWith("SELECT"))
                {
                    tokens.Add(new Token(TokenType.Select, "SELECT", _index));
                    _index += 6; continue;
                }
                if (_src.AsSpan(_index).StartsWith("EXECUTE"))
                {
                    tokens.Add(new Token(TokenType.Execute, "EXECUTE", _index));
                    _index += 7; continue;
                }
                if (_src.AsSpan(_index).StartsWith("WHERE"))
                {
                    tokens.Add(new Token(TokenType.Where, "WHERE", _index));
                    _index += 5; continue;
                }
                if (_src.AsSpan(_index).StartsWith("->"))
                {
                    tokens.Add(new Token(TokenType.Arrow, "->", _index));
                    _index += 2; continue;
                }
                if (current == '<')
                {
                    int start = _index;
                    while (_index < _src.Length && _src[_index] != '>') _index++;
                    if (_index < _src.Length) _index++; // Смещение закрывающей скобки
                    tokens.Add(new Token(TokenType.Selector, _src.Substring(start, _index - start), start));
                    continue;
                }

                // Выделение составных предикатов и функциональных методов действий
                int wordStart = _index;
                while (_index < _src.Length && _src[_index] != '-' && !char.IsWhiteSpace(_src[_index]))
                {
                    if (_src[_index] == '(')
                    {
                        while (_index < _src.Length && _src[_index] != ')') _index++;
                        if (_index < _src.Length) _index++;
                        break;
                    }
                    _index++;
                }

                string candidate = _src.Substring(wordStart, _index - wordStart);
                if (candidate.Contains("=") || candidate.StartsWith("Id='"))
                {
                    tokens.Add(new Token(TokenType.Predicate, candidate, wordStart));
                }
                else
                {
                    tokens.Add(new Token(TokenType.Action, candidate, wordStart));
                }
            }

            tokens.Add(new Token(TokenType.EOF, "", _index));
            return tokens;
        }
    }
}