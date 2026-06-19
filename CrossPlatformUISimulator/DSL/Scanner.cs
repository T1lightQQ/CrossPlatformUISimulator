using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.DSL
{
   
    public class Scanner
    {
        private readonly string _source;
        private int _index = 0;

        public Scanner(string source) => _source = source;

        public List<Token> ScanTokens()
        {
            var tokens = new List<Token>();
            while (_index < _source.Length)
            {
                char c = _source[_index];
                if (char.IsWhiteSpace(c)) { _index++; continue; }

                if (_source.AsSpan(_index).StartsWith("SELECT")) { tokens.Add(new Token(TokenType.Select, "SELECT", _index)); _index += 6; continue; }
                if (_source.AsSpan(_index).StartsWith("EXECUTE")) { tokens.Add(new Token(TokenType.Execute, "EXECUTE", _index)); _index += 7; continue; }
                if (_source.AsSpan(_index).StartsWith("WHERE")) { tokens.Add(new Token(TokenType.Where, "WHERE", _index)); _index += 5; continue; }
                if (_source.AsSpan(_index).StartsWith("->")) { tokens.Add(new Token(TokenType.Arrow, "->", _index)); _index += 2; continue; }

                if (c == '<')
                {
                    int start = _index;
                    while (_index < _source.Length && _source[_index] != '>') _index++;
                    if (_index < _source.Length) _index++;
                    tokens.Add(new Token(TokenType.Selector, _source.Substring(start, _index - start), start));
                    continue;
                }

                int wordStart = _index;
                while (_index < _source.Length && _source[_index] != '-' && !char.IsWhiteSpace(_source[_index]))
                {
                    if (_source[_index] == '(')
                    {
                        while (_index < _source.Length && _source[_index] != ')') _index++;
                        if (_index < _source.Length) _index++;
                        break;
                    }
                    _index++;
                }

                string candidate = _source.Substring(wordStart, _index - wordStart);
                tokens.Add(candidate.Contains("=")
                    ? new Token(TokenType.Predicate, candidate, wordStart)
                    : new Token(TokenType.Action, candidate, wordStart));
            }
            tokens.Add(new Token(TokenType.EOF, "", _index));
            return tokens;
        }
    }
}