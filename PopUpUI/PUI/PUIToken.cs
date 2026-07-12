using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public enum TokenType
    {
        DefineClause,
        CustomElementClause,
        Identifier,

        Integer,
        Float,
        Double,
        String,
        Boolean,

        Comma,
        Semicolon,
        Equals,
        Addition,

        LeftParen, // (
        RightParen,

        LeftBracket, // [
        RightBracket,

        LeftBrace, // {
        RightBrace,

        EndOfFile
    }

    public readonly struct PUIToken(TokenType type, string text)
    {
        public TokenType Type { get; } = type;
        public string Text { get; } = text;
    }
}
