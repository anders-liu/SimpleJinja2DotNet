using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static SimpleJinja2DotNet.TextUtils;

namespace SimpleJinja2DotNet
{
    internal class Parser
    {
        internal Parser(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                _s = string.Empty;
                _l = 0;
            }
            else
            {
                _lex = new Lexer(s);
                _s = s;
                _l = s.Length;
            }
        }

        internal IEnumerable<Block> Parse()
        {
            if (string.IsNullOrWhiteSpace(_s))
                return Array.Empty<Block>();

            var clauseList = ParseClauseList(0);
            var blockList = ParseBlockList(clauseList);
            return blockList;
        }

        #region Clause
        private IList<Clause> ParseClauseList(int i)
        {
            var clauseList = new List<Clause>();
            Clause c;
            do
            {
                c = ParseClause(i);
                if (c != null)
                {
                    clauseList.Add(c);
                    i = c.End;
                }
            } while (c != null);

            return clauseList;
        }

        private Clause ParseClause(int i)
        {
            switch (_lex.GetToken(i))
            {
                case Token t when t.IsInvalid:
                    return null;
                case Token t when t.TokenType == TokenType.StatementBegin:
                    return ParseStatementClause(t);
                case Token t when t.TokenType == TokenType.ExpressionBegin:
                    return ParseExpressionClause(t);
                default:
                    return ParseTextClause(i);
            }
        }

        private Clause ParseStatementClause(Token initialToken)
        {
            Debug.Assert(initialToken.TokenType == TokenType.StatementBegin);

            Token matchStatementEnd(int i)
            {
                var endToken = _lex.GetToken(i);
                if (endToken.TokenType != TokenType.StatementEnd)
                    throw ThrowHelper(i, ParsingError.IncompletedStatementBlock);
                return endToken;
            }

            var t = _lex.GetToken(initialToken.End);
            switch (t.TokenType)
            {
                case TokenType.If:
                    {
                        var test = ParseExpression(t.End);
                        if (test == null)
                            throw ThrowHelper(t.End, ParsingError.MissingTestExpression);
                        var endToken = matchStatementEnd(test.End);
                        return new IfClause(initialToken.Start, endToken.End, test);
                    }
                case TokenType.ElseIf:
                    {
                        var test = ParseExpression(t.End);
                        if (test == null)
                            throw ThrowHelper(t.End, ParsingError.MissingTestExpression);
                        var endToken = matchStatementEnd(test.End);
                        return new ElseIfClause(initialToken.Start, endToken.End, test);
                    }
                case TokenType.Else:
                    {
                        var endToken = matchStatementEnd(t.End);
                        return new ElseClause(initialToken.Start, endToken.End);
                    }
                case TokenType.EndIf:
                    {
                        var endToken = matchStatementEnd(t.End);
                        return new EndIfClause(initialToken.Start, endToken.End);
                    }
                case TokenType.For:
                    {
                        var loopVariable = ParseExpression(t.End);
                        if (loopVariable == null || !(loopVariable is SymbolExpression))
                            throw ThrowHelper(t.End, ParsingError.MissingLoopVariable);

                        var inToken = _lex.GetToken(loopVariable.End);
                        if (inToken.TokenType != TokenType.In)
                            throw ThrowHelper(loopVariable.End, ParsingError.MissingInKeyword);

                        var iterator = ParseAtomExpressionExpression(inToken.End);
                        if (iterator == null)
                            throw ThrowHelper(inToken.End, ParsingError.MissingIterator);

                        var endToken = matchStatementEnd(iterator.End);
                        return new ForClause(initialToken.Start, endToken.End,
                            loopVariable, iterator);
                    }
                case TokenType.EndFor:
                    {
                        var endToken = matchStatementEnd(t.End);
                        return new EndForClause(initialToken.Start, endToken.End);
                    }
                default:
                    throw ThrowHelper(t.Start, ParsingError.UnknownStatementType);
            }
        }

        private Clause ParseExpressionClause(Token initialToken)
        {
            Debug.Assert(initialToken.TokenType == TokenType.ExpressionBegin);

            var expression = ParseExpression(initialToken.End);
            if (expression == null)
                throw ThrowHelper(initialToken.Start, ParsingError.IncompletedExpressionBlock);

            var endToken = _lex.GetToken(expression.End);

            if (endToken.TokenType != TokenType.ExpressionEnd)
                throw ThrowHelper(initialToken.Start, ParsingError.IncompletedExpressionBlock);

            return new ExpressionClause(initialToken.Start, endToken.End,
                expression);
        }

        private Clause ParseTextClause(int i)
        {
            var end = i;
            Token t = _lex.GetToken(end);
            while ((!t.IsInvalid
                && t.TokenType != TokenType.StatementBegin
                && t.TokenType != TokenType.ExpressionBegin)
                || (t.IsInvalid && end < _l))
            {
                end = t.IsInvalid ? end + 1 : t.End;
                t = _lex.GetToken(end);
            }

            return new TextClause(i, end);
        }
        #endregion Clause

        #region Expression
        private Expression ParseExpression(int i)
        {
            return ParseFilterExpression(i);
        }

        private Expression ParseFilterExpression(int i)
        {
            var e = ParseTestExpression(i);
            if (e == null)
                return null;

            var t = _lex.GetToken(e.End);
            while (t.TokenType == TokenType.Pipe)
            {
                var r = ParseFilterCallExpression(t.End);
                if (r == null)
                    throw ThrowHelper(t.End, ParsingError.MissingFilterCall);

                e = new BinaryExpression(e.Start, r.End,
                    BinaryOperator.Pipe, e, r);
                t = _lex.GetToken(r.End);
            }

            return e;
        }

        private Expression ParseFilterCallExpression(int i)
        {
            var e = ParseExpression(i);
            if (e == null)
                return null;

            if (!(e is SymbolExpression))
                throw ThrowHelper(e.Start, ParsingError.InvalidFilter);

            var argList = ParseArgumentListExpression(e.End);
            if (argList != null)
                e = new BinaryExpression(e.Start, argList.End,
                    BinaryOperator.FunctionCall, e, argList);

            return e;
        }

        private Expression ParseArgumentListExpression(int i)
        {
            var startToken = _lex.GetToken(i);
            if (startToken.TokenType != TokenType.LeftParenthesis)
                return null;

            var endTokenIndex = startToken.End;
            var expressionList = new List<Expression>();
            var expression = ParseTestExpression(startToken.End);
            if (expression != null)
            {
                endTokenIndex = expression.End;
                expressionList.Add(expression);

                var separatorToken = _lex.GetToken(expression.End);
                while (separatorToken.TokenType == TokenType.Comma)
                {
                    expression = ParseTestExpression(separatorToken.End);
                    if (expression == null)
                        throw ThrowHelper(separatorToken.End, ParsingError.MissingExpression);

                    endTokenIndex = expression.End;
                    expressionList.Add(expression);
                    separatorToken = _lex.GetToken(expression.End);
                }
            }

            var endToken = _lex.GetToken(endTokenIndex);
            if (endToken.TokenType != TokenType.RightParenthesis)
                throw ThrowHelper(endTokenIndex, ParsingError.MissingParenthesis);

            return new ListExpression(startToken.Start, endToken.End,
                expressionList);
        }

        private Expression ParseTestExpression(int i)
        {
            return ParseOrTestExpression(i);
        }

        private Expression ParseOrTestExpression(int i)
        {
            var e = ParseAndTestExpression(i);
            if (e == null)
                return null;

            var t = _lex.GetToken(e.End);
            while (t.TokenType == TokenType.Or)
            {
                var r = ParseAndTestExpression(t.End);
                if (r == null)
                    throw ThrowHelper(t.End, ParsingError.MissingExpression);

                e = new BinaryExpression(e.Start, r.End,
                    BinaryOperator.Or, e, r);
                t = _lex.GetToken(r.End);
            }

            return e;
        }

        private Expression ParseAndTestExpression(int i)
        {
            var e = ParseNotExpression(i);
            if (e == null)
                return null;

            var t = _lex.GetToken(e.End);
            while (t.TokenType == TokenType.And)
            {
                var r = ParseNotExpression(t.End);
                if (r == null)
                    throw ThrowHelper(t.End, ParsingError.MissingExpression);

                e = new BinaryExpression(e.Start, r.End,
                    BinaryOperator.And, e, r);
                t = _lex.GetToken(r.End);
            }

            return e;
        }

        private Expression ParseNotExpression(int i)
        {
            var t = _lex.GetToken(i);
            if (t.TokenType == TokenType.Not)
            {
                var e = ParseNotExpression(t.End);
                if (e == null)
                    throw ThrowHelper(t.End, ParsingError.MissingExpression);

                return new UnaryExpression(t.Start, e.End,
                    UnaryOperator.Not, e);
            }
            else
            {
                return ParseCompareExpression(i);
            }
        }

        private Expression ParseCompareExpression(int i)
        {
            var e = ParseMathExpression(i);
            if (e == null)
                return null;

            var t = _lex.GetToken(e.End);
            while (t.IsCompareOperator)
            {
                var r = ParseMathExpression(t.End);
                if (r == null)
                    throw ThrowHelper(t.End, ParsingError.MissingExpression);

                e = new BinaryExpression(e.Start, r.End,
                    t.AsBinaryOperator, e, r);
                t = _lex.GetToken(r.End);
            }

            return e;
        }

        private Expression ParseMathExpression(int i)
        {
            var e = ParseTermExpression(i);
            if (e == null)
                return null;

            var t = _lex.GetToken(e.End);
            while (t.IsTermOperator)
            {
                var r = ParseTermExpression(t.End);
                if (r == null)
                    throw ThrowHelper(t.End, ParsingError.MissingExpression);

                e = new BinaryExpression(e.Start, r.End,
                    t.AsBinaryOperator, e, r);
                t = _lex.GetToken(r.End);
            }

            return e;
        }

        private Expression ParseTermExpression(int i)
        {
            var e = ParseFactorExpression(i);
            if (e == null)
                return null;

            var t = _lex.GetToken(e.End);
            while (t.IsFactorOperator)
            {
                var r = ParseFactorExpression(t.End);
                if (r == null)
                    throw ThrowHelper(t.End, ParsingError.MissingExpression);

                e = new BinaryExpression(e.Start, r.End,
                    t.AsBinaryOperator, e, r);
                t = _lex.GetToken(r.End);
            }

            return e;
        }

        private Expression ParseFactorExpression(int i)
        {
            var t = _lex.GetToken(i);

            if (t.IsAtomExpressionOperator)
            {
                var e = ParseAtomExpressionExpression(t.End);
                if (e == null)
                    throw ThrowHelper(t.End, ParsingError.MissingExpression);

                return new UnaryExpression(t.Start, e.End,
                    t.AsUnaryOperator, e);
            }
            else
            {
                return ParseAtomExpressionExpression(i);
            }
        }

        private Expression ParseAtomExpressionExpression(int i)
        {
            var e = ParseAtomExpression(i);
            if (e == null)
                return null;

            var t = _lex.GetToken(e.End);

            while (t.TokenType == TokenType.LeftSquareBracket
                || t.TokenType == TokenType.Dot)
            {
                if (t.TokenType == TokenType.LeftSquareBracket)
                {
                    var r = ParseExpression(t.End);
                    if (r == null)
                        throw ThrowHelper(t.End, ParsingError.MissingExpression);

                    var endToken = _lex.GetToken(r.End);
                    if (endToken.TokenType != TokenType.RightSquareBracket)
                        throw ThrowHelper(r.End, ParsingError.MissingEndOfSubscript);

                    e = new BinaryExpression(e.Start, endToken.End,
                        BinaryOperator.Subscript, e, r);
                    t = _lex.GetToken(endToken.End);
                }
                else if (t.TokenType == TokenType.Dot)
                {
                    var tr = _lex.GetToken(t.End);
                    if (tr.TokenType != TokenType.Symbol)
                        throw ThrowHelper(t.End, ParsingError.InvalidSymbol);

                    var r = new SymbolExpression(tr.Start, tr.End,
                        _s.Substring(tr.Start, tr.End - tr.Start));
                    e = new BinaryExpression(e.Start, r.End,
                        BinaryOperator.MemberAccess, e, r);
                    t = _lex.GetToken(r.End);
                }
            }

            return e;
        }

        private Expression ParseAtomExpression(int i)
        {
            var t = _lex.GetToken(i);
            if (t.IsInvalid)
                return null;

            switch (t.TokenType)
            {
                case TokenType.Symbol:
                    {
                        var str = _s.Substring(t.Start, t.End - t.Start);
                        return new SymbolExpression(t.Start, t.End, str);
                    }
                case TokenType.String:
                    {
                        var str = _s.Substring(t.Start, t.End - t.Start);
                        Debug.Assert(str.Length >= 2);
                        var content = str.Substring(1, str.Length - 2);
                        int.TryParse(content, out var intValue);
                        float.TryParse(content, out var floatValue);
                        var boolValue = !string.IsNullOrEmpty(content);
                        return new LiteralExpression(t.Start, t.End,
                            ValueType.String, content, intValue, floatValue, boolValue);
                    }
                case TokenType.Integer:
                    {
                        var str = _s.Substring(t.Start, t.End - t.Start);
                        if (!int.TryParse(str, out var intValue))
                            throw ThrowHelper(t.Start, ParsingError.InvalidNumber);
                        var floatValue = (float)intValue;
                        var boolValue = intValue != 0;
                        return new LiteralExpression(t.Start, t.End,
                            ValueType.Integer, str, intValue, floatValue, boolValue);
                    }
                case TokenType.Float:
                    {
                        var str = _s.Substring(t.Start, t.End - t.Start);
                        if (!float.TryParse(str, out var floatValue))
                            throw ThrowHelper(t.Start, ParsingError.InvalidNumber);
                        var intValue = (int)floatValue;
                        var boolValue = intValue != 0;
                        return new LiteralExpression(t.Start, t.End,
                            ValueType.Float, str, intValue, floatValue, boolValue);
                    }
                case TokenType.True:
                    {
                        var str = _s.Substring(t.Start, t.End - t.Start);
                        var intValue = 1;
                        var floatValue = 1.0;
                        return new LiteralExpression(t.Start, t.End,
                            ValueType.Boolean, str, intValue, floatValue, true);
                    }
                case TokenType.False:
                    {
                        var str = _s.Substring(t.Start, t.End - t.Start);
                        var intValue = 0;
                        var floatValue = 0.0;
                        return new LiteralExpression(t.Start, t.End,
                            ValueType.Boolean, str, intValue, floatValue, false);
                    }
                default:
                    return ParseParenthesisExpression(i);
            }
        }

        private Expression ParseParenthesisExpression(int i)
        {
            var startToken = _lex.GetToken(i);
            if (startToken.TokenType != TokenType.LeftParenthesis)
                return null;

            var e = ParseExpression(startToken.End);
            if (e == null)
                throw ThrowHelper(startToken.End, ParsingError.MissingExpression);

            var endToken = _lex.GetToken(e.End);
            if (endToken.TokenType != TokenType.RightParenthesis)
                throw ThrowHelper(e.End, ParsingError.MissingParenthesis);

            return new ParenthesisExpression(startToken.Start, endToken.End, e);
        }
        #endregion Expression

        #region Block
        private IList<Block> ParseBlockList(IList<Clause> clauseList)
        {
            var i = 0;
            return ParseBlockList(clauseList, ref i);
        }

        private IList<Block> ParseBlockList(IList<Clause> clauseList, ref int i)
        {
            var blockList = new List<Block>();
            var encounteredBlockDelimiter = false;
            while (i < clauseList.Count && !encounteredBlockDelimiter)
            {
                switch (clauseList[i])
                {
                    case TextClause _:
                        var textBlock = ParseTextBlock(clauseList, ref i);
                        blockList.Add(textBlock);
                        break;
                    case IfClause _:
                        var ifBlock = ParseIfBlock(clauseList, ref i);
                        blockList.Add(ifBlock);
                        break;
                    case ForClause _:
                        var forBlock = ParseForBlock(clauseList, ref i);
                        blockList.Add(forBlock);
                        break;
                    case ElseIfClause _:
                    case ElseClause _:
                    case EndIfClause _:
                    case EndForClause _:
                        encounteredBlockDelimiter = true;
                        break;
                    case ExpressionClause c:
                        var expressionBlock = new ExpressionBlock(c.Start, c.End,
                            c.Expression);
                        i++;
                        blockList.Add(expressionBlock);
                        break;
                    default:
                        throw ThrowHelper(clauseList[i].Start, ParsingError.UnknownClause);
                }
            }

            return blockList;
        }

        private Block ParseTextBlock(IList<Clause> clauseList, ref int i)
        {
            var c = clauseList[i++];

            return new TextBlock(c.Start, c.End,
                _s.Substring(c.Start, c.End - c.Start));
        }

        private Block ParseIfBlock(IList<Clause> clauseList, ref int i)
        {
            var testList = new List<TestBlock>();

            var ifClause = clauseList[i] as IfClause;
            i++;
            var ifTestBody = ParseBlockList(clauseList, ref i);
            var end = ifTestBody.Count > 0
                ? ifTestBody[ifTestBody.Count - 1].End
                : ifClause.End;
            var ifTestBlock = new TestBlock(ifClause.Start, end,
                ifClause.Test, ifTestBody);
            testList.Add(ifTestBlock);

            while (i < clauseList.Count && clauseList[i] is ElseIfClause)
            {
                var elseIfClause = clauseList[i] as ElseIfClause;
                i++;
                var elseIfTestBody = ParseBlockList(clauseList, ref i);
                end = elseIfTestBody.Count > 0
                    ? elseIfTestBody.Last().End
                    : elseIfClause.End;
                var elseIfTestBlock = new TestBlock(elseIfClause.Start, end,
                    elseIfClause.Test, elseIfTestBody);
                testList.Add(elseIfTestBlock);
            }

            if (i < clauseList.Count && clauseList[i] is ElseClause)
            {
                var elseClause = clauseList[i] as ElseClause;
                i++;
                var elseTestBody = ParseBlockList(clauseList, ref i);
                end = elseTestBody.Count > 0
                    ? elseTestBody.Last().End
                    : elseClause.End;
                var elseTestBlock = new TestBlock(elseClause.Start, end,
                    null, elseTestBody);
                testList.Add(elseTestBlock);
            }

            if (i < clauseList.Count && clauseList[i] is EndIfClause)
            {
                var endIfClause = clauseList[i] as EndIfClause;
                i++;
                return new IfStatementBlock(ifClause.Start, endIfClause.End,
                    testList);
            }
            else
            {
                throw ThrowHelper(ifClause.Start, ParsingError.MissingEndIf);
            }
        }

        private Block ParseForBlock(IList<Clause> clauseList, ref int i)
        {
            var forClause = clauseList[i] as ForClause;
            i++;
            var forBody = ParseBlockList(clauseList, ref i);

            if (i < clauseList.Count && clauseList[i] is EndForClause)
            {
                var endForClause = clauseList[i] as EndForClause;
                i++;
                return new ForStatementBlock(forClause.Start, endForClause.End,
                    forClause.LoopVariable, forClause.Iterator, forBody);
            }
            else
            {
                throw ThrowHelper(forClause.Start, ParsingError.MissingEndFor);
            }
        }
        #endregion Block

        private Exception ThrowHelper(int charIndex, ParsingError error)
        {
            (var row, var column) = GetCharPosition(_s, charIndex);
            return new ParsingException(charIndex, row, column, error);
        }

        private readonly Lexer _lex;
        private readonly string _s;
        private readonly int _l;
    }
}
