using PopUpUI.PUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    internal static class PUILexer
    {
        public static List<PUIToken> Tokenize(string input, int count)
        {
            List<PUIToken> tokens = new List<PUIToken>();
            int position = 0;

            while (position < count)
            {
                char c = input[position];

                if (char.IsWhiteSpace(c))
                {
                    position++;
                    continue;
                }

                // single line comment
                if (c == '/' && position + 1 < count && input[position + 1] == '/')
                {
                    position += 2;

                    while (position < count && input[position] != '\n')
                        position++;

                    continue;
                }

                // multiline commend
                if (c == '/' && position + 1 < count && input[position + 1] == '*')
                {
                    position += 2;

                    while (position + 1 < count)
                    {
                        if (input[position] == '*' && input[position + 1] == '/')
                        {
                            position += 2;
                            break;
                        }
                    }

                    position += 1;
                }

                switch (c)
                {
                    case ',':
                        tokens.Add(new PUIToken(TokenType.Comma, ","));
                        position++;
                        continue;

                    case '(':
                        tokens.Add(new PUIToken(TokenType.LeftParen, "("));
                        position++;
                        continue;

                    case ')':
                        tokens.Add(new PUIToken(TokenType.RightParen, ")"));
                        position++;
                        continue;

                    case '[':
                        tokens.Add(new PUIToken(TokenType.LeftBracket, "["));
                        position++;
                        continue;

                    case ']':
                        tokens.Add(new PUIToken(TokenType.RightBracket, "]"));
                        position++;
                        continue;

                    case '{':
                        tokens.Add(new PUIToken(TokenType.LeftBrace, "{"));
                        position++;
                        continue;

                    case '}':
                        tokens.Add(new PUIToken(TokenType.RightBrace, "}"));
                        position++;
                        continue;

                    case ';':
                        tokens.Add(new PUIToken(TokenType.Semicolon, ";"));
                        position++;
                        continue;

                    case '+':
                        tokens.Add(new PUIToken(TokenType.Addition, "+"));
                        position++;
                        continue;

                    case '=':
                        tokens.Add(new PUIToken(TokenType.Equals, "="));
                        position++;
                        continue;
                }

                if (c == '#')
                {
                    position++;

                    const int DefineCharCount = 6;
                    if (position + DefineCharCount < count && input.Substring(position, DefineCharCount) == "define")
                    {
                        tokens.Add(new PUIToken(TokenType.DefineClause, "#define"));
                        position += DefineCharCount;
                        continue;
                    }

                    const int CustomElementCharCount = 13;
                    if (position + CustomElementCharCount < count && input.Substring(position, CustomElementCharCount) == "customelement")
                    {
                        tokens.Add(new PUIToken(TokenType.CustomElementClause, "#customelement"));
                        position += CustomElementCharCount;
                        continue;
                    }
                }

                if (c == '"')
                {
                    position++;

                    StringBuilder builder = new StringBuilder();
                    while (position < count)
                    {
                        if (input[position] == '\\' && position + 1 < count)
                        {
                            char escapeChar = input[position + 1];
                            switch (escapeChar)
                            {
                                case 'n':
                                    builder.Append('\n');
                                    position += 2;
                                    continue;

                                case 't':
                                    builder.Append('\t');
                                    position += 2;
                                    continue;

                                default:
                                    builder.Append('\\');
                                    position += 1;
                                    continue;
                            }
                        }

                        if (input[position] == '"')
                        {
                            position++;
                            break;
                        }

                        builder.Append(input[position]);
                        position++;
                    }

                    if (input[position - 1] != '"')
                        throw new LexerException($"Double quatos did not closed properly. Last processed index: {position}");

                    tokens.Add(new PUIToken(TokenType.String, builder.ToString()));
                    continue;
                }

                if (char.IsDigit(c))
                {
                    int start = position;
                    bool hasDecimal = false;

                    while (position < count)
                    {
                        if (char.IsDigit(input[position]))
                        {
                            position++;
                            continue;
                        }

                        if (input[position] == '.' && !hasDecimal)
                        {
                            hasDecimal = true;
                            position++;
                            continue;
                        }

                        break;
                    }

                    if (position < count)
                    {
                        char suffix = input[position];

                        if (suffix == 'f' || suffix == 'F')
                        {
                            string value = input[start..position];
                            tokens.Add(new PUIToken(TokenType.Float, value));

                            position++;
                            continue;

                        }
                        else if (suffix == 'd' || suffix == 'D')
                        {
                            string value = input[start..position];
                            tokens.Add(new PUIToken(TokenType.Double, value));

                            position++;
                            continue;
                        }
                    }

                    string number = input[start..position];
                    if (hasDecimal)
                    {
                        tokens.Add(new PUIToken(TokenType.Double, number));
                    }
                    else
                    {
                        tokens.Add(new PUIToken(TokenType.Integer, number));
                    }

                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    int start = position;

                    while (position < count && (char.IsLetterOrDigit(input[position]) || input[position] == '_'))
                        position++;

                    string text = input[start..position];

                    switch (text)
                    {
                        case "true":
                        case "false":
                            tokens.Add(new PUIToken(TokenType.Boolean, text));
                            continue;
                        default:
                            tokens.Add(new PUIToken(TokenType.Identifier, text));
                            continue;
                    }
                }

                throw new LexerException($"Unexpected character {c} at index: {position}");
            }

            tokens.Add(new PUIToken(TokenType.EndOfFile, "\0"));
            return tokens;
        }
    }
}
