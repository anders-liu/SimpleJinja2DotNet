using System.Diagnostics;

namespace SimpleJinja2DotNet
{
    internal class Lexer
    {
        internal Lexer(string s)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(s));

            _s = s;
            _l = s.Length;
        }

        internal Token GetToken(int i)
        {
            var start = SkipWhitespaces(i);

            switch (CharAt(start))
            {
                case '{':
                    switch (CharAt(start + 1))
                    {
                        case '%':
                            return new Token(start, start + 2, TokenType.StatementBegin);
                        case '{':
                            return new Token(start, start + 2, TokenType.ExpressionBegin);
                        default:
                            return Token.Invalid(start);
                    }

                case '%':
                    switch (CharAt(start + 1))
                    {
                        case '}':
                            return new Token(start, start + 2, TokenType.StatementEnd);
                        default:
                            return new Token(start, start + 1, TokenType.Percent);
                    }

                case '}':
                    switch (CharAt(start + 1))
                    {
                        case '}':
                            return new Token(start, start + 2, TokenType.ExpressionEnd);
                        default:
                            return Token.Invalid(start);
                    }

                case '+':
                    return new Token(start, start + 1, TokenType.Plus);

                case '-':
                    return new Token(start, start + 1, TokenType.Minus);

                case '|':
                    return new Token(start, start + 1, TokenType.Pipe);

                case '(':
                    return new Token(start, start + 1, TokenType.LeftParenthesis);

                case ')':
                    return new Token(start, start + 1, TokenType.RightParenthesis);

                case '[':
                    return new Token(start, start + 1, TokenType.LeftSquareBracket);

                case ']':
                    return new Token(start, start + 1, TokenType.RightSquareBracket);

                case '.':
                    switch (CharAt(start + 1))
                    {
                        case char c when char.IsDigit(c):
                            return GetDotStartedFloat(start);
                        default:
                            return new Token(start, start + 1, TokenType.Dot);
                    }

                case '*':
                    return new Token(start, start + 1, TokenType.Star);

                case '/':
                    switch (CharAt(start + 1))
                    {
                        case '/':
                            return new Token(start, start + 2, TokenType.DoubleSlash);
                        default:
                            return new Token(start, start + 1, TokenType.Slash);
                    }

                case '<':
                    switch (CharAt(start + 1))
                    {
                        case '=':
                            return new Token(start, start + 2, TokenType.LessEqual);
                        default:
                            return new Token(start, start + 1, TokenType.Less);
                    }

                case '=':
                    switch (CharAt(start + 1))
                    {
                        case '=':
                            return new Token(start, start + 2, TokenType.DoubleEqual);
                        default:
                            return Token.Invalid(start);
                    }

                case '>':
                    switch (CharAt(start + 1))
                    {
                        case '=':
                            return new Token(start, start + 2, TokenType.GreaterEqual);
                        default:
                            return new Token(start, start + 1, TokenType.Greater);
                    }

                case '!':
                    switch (CharAt(start + 1))
                    {
                        case '=':
                            return new Token(start, start + 2, TokenType.ExclamationEqual);
                        default:
                            return Token.Invalid(start);
                    }

                case '\'':
                    return GetString(start);

                case ',':
                    return new Token(start, start + 1, TokenType.Comma);

                case '_':
                case char c when char.IsLetter(c):
                    return GetSymbolOrKeyword(start);

                case char c when char.IsDigit(c):
                    return GetIntegerOrFloat(start);
            }

            return Token.Invalid(start);
        }

        private Token GetString(int start)
        {
            Debug.Assert(CharAt(start) == '\'');

            var i = start + 1;
            while (CharAt(i) != '\0' && CharAt(i) != '\'')
                i++;
            return new Token(start, i + 1, TokenType.String);
        }

        private Token GetDotStartedFloat(int start)
        {
            Debug.Assert(CharAt(start) == '.');

            var i = start + 1;
            while (char.IsDigit(CharAt(i)))
                i++;
            return new Token(start, i, TokenType.Float);
        }

        private Token GetIntegerOrFloat(int start)
        {
            Debug.Assert(char.IsDigit(CharAt(start)));

            var i = start + 1;
            while (char.IsDigit(CharAt(i)))
                i++;

            if (CharAt(i) == '.' && char.IsDigit(CharAt(i + 1)))
            {
                i++;
                while (char.IsDigit(CharAt(i)))
                    i++;
                return new Token(start, i, TokenType.Float);
            }
            else
            {
                return new Token(start, i, TokenType.Integer);
            }
        }

        private Token GetSymbolOrKeyword(int start)
        {
            Debug.Assert(CharAt(start) == '_' || char.IsLetter(CharAt(start)));

            var i = start + 1;
            while (CharAt(i) == '_' || char.IsDigit(CharAt(i)) || char.IsLetter(CharAt(i)))
                i++;

            var str = _s.Substring(start, i - start);
            switch (str)
            {
                case "if": return new Token(start, i, TokenType.If);
                case "elif": return new Token(start, i, TokenType.ElseIf);
                case "else": return new Token(start, i, TokenType.Else);
                case "endif": return new Token(start, i, TokenType.EndIf);
                case "for": return new Token(start, i, TokenType.For);
                case "in": return new Token(start, i, TokenType.In);
                case "endfor": return new Token(start, i, TokenType.EndFor);
                case "not": return new Token(start, i, TokenType.Not);
                case "or": return new Token(start, i, TokenType.Or);
                case "and": return new Token(start, i, TokenType.And);
                case "true": return new Token(start, i, TokenType.True);
                case "false": return new Token(start, i, TokenType.False);
                default: return new Token(start, i, TokenType.Symbol);
            }
        }

        private int SkipWhitespaces(int i)
        {
            var e = i;
            while (char.IsWhiteSpace(CharAt(e)))
                e++;
            return e;
        }

        private char CharAt(int i)
        {
            return i >= 0 && i < _l ? _s[i] : '\0';
        }

        private readonly string _s;
        private readonly int _l;
    }
}
