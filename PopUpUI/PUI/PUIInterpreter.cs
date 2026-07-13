using System;
using System.Collections.Generic;
using System.IO;

namespace PopUpUI.PUI
{
    public class PUIInterpreter
    {
        private readonly TokenCursor cursor;
        private readonly Dictionary<string, IPUIVariableCreator> _variableCreators;
        private readonly Dictionary<string, ITupleParser> _tupleParsers;

        private readonly Dictionary<string, PUIVariable> _loadedVariables;
        private readonly Dictionary<string, PUIDescriptor> _loadedDescriptors;

        public IEnumerable<IPUIVariableCreator> VariableCreators => _variableCreators.Values;
        public IEnumerable<ITupleParser> TupleParsers => _tupleParsers.Values;

        public PUIInterpreter(string path)
        {
            string s = File.ReadAllText(path);
            List<PUIToken> tokens = PUILexer.Tokenize(s, s.Length);

            cursor = new TokenCursor(tokens);
            _variableCreators = new Dictionary<string, IPUIVariableCreator>();
            _tupleParsers = new Dictionary<string, ITupleParser>();
            _loadedVariables = new Dictionary<string, PUIVariable>();
            _loadedDescriptors = new Dictionary<string, PUIDescriptor>();
        }

        public void AssignProperties<T>(IDescriptorAssigner<T> assigner, string name, T element)
        {
            if (!_loadedDescriptors.TryGetValue(name, out PUIDescriptor desc) || desc.TypeName != assigner.TypeName)
            {
                throw new ArgumentException($"Types of given assigner and descriptor with name \"{name}\" does not match.");
            }

            assigner.Assign(desc, element);
        }

        public T CreateFromProperties<T>(IDescriptorInstantiator<T> instantiator, string name)
        {
            if (!_loadedDescriptors.TryGetValue(name, out PUIDescriptor desc) || desc.TypeName != instantiator.TypeName)
            {
                throw new ArgumentException($"Types of given instantiator and descriptor with name \"{name}\" does not match.");
            }

            return instantiator.Instantiate(desc);
        }

        public void AddVariableCreators(IEnumerable<IPUIVariableCreator> variableCreators)
        {
            foreach (var creator in variableCreators) _variableCreators.Add(creator.TypeName, creator);
        }

        public void AddVariableCreators(params ReadOnlySpan<IPUIVariableCreator> variableCreators)
        {
            foreach (var creator in variableCreators) _variableCreators.Add(creator.TypeName, creator);
        }

        public void AddTupleParsers(IEnumerable<ITupleParser> tupleParsers)
        {
            foreach (var parser in tupleParsers) _tupleParsers.Add(parser.TypeName, parser);
        }

        public void AddTupleParsers(params ReadOnlySpan<ITupleParser> tupleParsers)
        {
            foreach (var parser in tupleParsers) _tupleParsers.Add(parser.TypeName, parser);
        }

        public void ProcessTokens()
        {
            while (cursor.Current.Type != TokenType.EndOfFile)
            {
                PUIToken token = cursor.Advance();

                if (token.Type == TokenType.DefineClause)
                {
                    cursor.ExpectNumberOfTokens(2);
                    string type = cursor.Expect(TokenType.Identifier).Text;
                    string variableName = cursor.Expect(TokenType.Identifier).Text;

                    TokenType next = cursor.Current.Type;

                    // if define clause directly equals to a value
                    if (next == TokenType.Equals)
                    {
                        cursor.ExpectNumberOfTokens(2);
                        cursor.Advance();

                        PUIVariable variable = _variableCreators[type].Get();
                        variable.Value = ReadValue();

                        cursor.Expect(TokenType.Semicolon);

                        _loadedVariables.Add(variableName, variable);
                        continue;
                    }
                    else if (next == TokenType.LeftBrace)
                    {
                        PUIBraceVariable variable = _variableCreators[type].Get() as PUIBraceVariable;
                        ReadBraceProperties(variable);

                        _loadedVariables.Add(variableName, variable);
                        continue;
                    }
                }

                if (token.Type == TokenType.Identifier)
                {
                    cursor.ExpectNumberOfTokens(2);
                    string elementName = cursor.Expect(TokenType.Identifier).Text;

                    PUIDescriptor desc = new PUIDescriptor(token.Text, elementName);
                    ReadBraceProperties(desc);

                    _loadedDescriptors.Add(desc.ElementName, desc);
                    continue;
                }
            }
        }

        // cursor is at left brace
        private void ReadBraceProperties(IBraceable braceOwner)
        {
            cursor.Expect(TokenType.LeftBrace);
            while (cursor.HasToken)
            {
                PUIToken token = cursor.Advance();

                if (token.Type == TokenType.RightBrace)
                {
                    break;
                }

                if (token.Type == TokenType.Identifier)
                {
                    cursor.ExpectNumberOfTokens(2); // '=' sign and value
                    cursor.Expect(TokenType.Equals);

                    braceOwner.AssignProperty(token.Text, ReadValue());

                    if (cursor.Current.Type != TokenType.RightBrace)
                    {
                        cursor.Expect(TokenType.Comma);
                    }

                    continue;
                }

                ThrowUnexpectedToken(token.Type);
            }
        }

        private object ReadValue()
        {
            if (cursor.PeekNext(1).Type == TokenType.Addition)
            {
                return ConcatValues();
            }
            else
            {
                return ReadSingleValue();
            }
        }

        private string ConcatValues()
        {
            PUIToken left = AsConcatableStringToken(cursor.Advance());
            while (cursor.Current.Type == TokenType.Addition)
            {
                cursor.Advance();
                PUIToken right = AsConcatableStringToken(cursor.Advance());

                left = new PUIToken(TokenType.String, left.Text + right.Text);
            }

            return left.Text;
        }

        private PUIToken AsConcatableStringToken(PUIToken token)
        {
            if (token.Type == TokenType.String)
            {
                return token;
            }
            else if (token.Type == TokenType.Identifier && _loadedVariables[token.Text].Value is string str)
            {
                return new PUIToken(TokenType.String, str);
            }
            else
            {
                throw new InterpreterException($"Addition is only supported with string values", cursor.CurrentIndex);
            }
        }

        /// <summary>Reads the value that comes after an equality sign '=' or TokenType.Equals.</summary>
        /// <returns></returns>
        private object ReadSingleValue()
        {
            PUIToken valueToken = cursor.Advance();

            if (valueToken.Type == TokenType.Boolean)
            {
                return valueToken.Text == "true";
            }

            if (valueToken.Type == TokenType.Integer)
            {
                return int.Parse(valueToken.Text);
            }

            if (valueToken.Type == TokenType.Float)
            {
                return float.Parse(valueToken.Text);
            }

            if (valueToken.Type == TokenType.Double)
            {
                return double.Parse(valueToken.Text);
            }

            if (valueToken.Type == TokenType.String)
            {
                return valueToken.Text;
            }

            cursor.ExpectNumberOfTokens(1);
            if (valueToken.Type == TokenType.Identifier)
            {
                PUIToken next = cursor.Current;

                if (next.Type == TokenType.LeftParen && _tupleParsers.TryGetValue(valueToken.Text, out ITupleParser parser))
                {
                    return parser.Parse(cursor);
                }
                else if (_loadedVariables.TryGetValue(valueToken.Text, out PUIVariable variable))
                {
                    return variable.Value;
                }
            }

            throw new InterpreterException($"Cannot resolve symbol {valueToken.Text}", cursor.CurrentIndex);
        }

        private void ThrowUnexpectedToken(TokenType type)
        {
            throw new InterpreterException($"Unexpected token type {type}", cursor.CurrentIndex);
        }
    }
}
