using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public class TokenCursor
    {
        private readonly List<PUIToken> _tokens;
        private int _index;

        public bool HasToken => _index < _tokens.Count;
        public int CurrentIndex => _index;
        public PUIToken Current => _tokens[_index];

        public TokenCursor(List<PUIToken> tokens)
        {
            _tokens = tokens;
            _index = 0;
        }

        public PUIToken Advance()
        {
            PUIToken t = Current;
            ++_index;
            return t;
        }

        public PUIToken PeekNext(int amount)
        {
            return _tokens[_index + amount];
        }

        public PUIToken Expect(TokenType type)
        {
            if (Current.Type != type)
            {
                throw new InterpreterException($"Expected: {type}, found: {Current.Type}", _index);
            }

            return Advance();
        }

        public void ExpectNumberOfTokens(int num)
        {
            if (!(_index + num < _tokens.Count))
            {
                throw new InterpreterException($"Insufficient number of tokens for statement, expected: {num}, found {_tokens.Count - _index - 1}", _index);
            }
        }
    }
}
