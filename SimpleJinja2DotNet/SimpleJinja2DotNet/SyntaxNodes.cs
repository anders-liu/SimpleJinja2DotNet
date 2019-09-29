using System;
using System.Collections.Generic;

namespace SimpleJinja2DotNet
{
    internal abstract class SyntaxNode
    {
        internal int Start { get; }

        internal int End { get; }

        internal protected SyntaxNode(int start, int end)
        {
            Start = start;
            End = end;
        }
    }

    #region Block
    internal abstract class Block : SyntaxNode
    {
        internal protected Block(int start, int end)
            : base(start, end) { }
    }

    internal class TextBlock : Block
    {
        internal TextBlock(int start, int end,
            string content)
            : base(start, end)
        {
            Content = content;
        }

        internal string Content { get; }
    }

    internal class IfStatementBlock : Block
    {
        internal IfStatementBlock(int start, int end,
            IEnumerable<TestBlock> testList)
            : base(start, end)
        {
            TestList = testList;
        }

        internal IEnumerable<TestBlock> TestList { get; }
    }

    internal class TestBlock : Block
    {
        internal TestBlock(int start, int end,
            Expression test, IEnumerable<Block> body)
            : base(start, end)
        {
            Test = test;
            Body = body;
        }

        internal Expression Test { get; }

        internal IEnumerable<Block> Body { get; }
    }

    internal class ForStatementBlock : Block
    {
        internal ForStatementBlock(int start, int end,
            Expression loopVariable, Expression iterator, IEnumerable<Block> body)
            : base(start, end)
        {
            LoopVariable = loopVariable;
            Iterator = iterator;
            Body = body;
        }

        internal Expression LoopVariable { get; }

        internal Expression Iterator { get; }

        internal IEnumerable<Block> Body { get; }
    }

    internal class ExpressionBlock : Block
    {
        internal ExpressionBlock(int start, int end,
            Expression expression)
            : base(start, end)
        {
            Expression = expression;
        }

        internal Expression Expression { get; }
    }
    #endregion Block

    #region Clause
    internal abstract class Clause : SyntaxNode
    {
        internal protected Clause(int start, int end)
            : base(start, end) { }
    }

    internal class TextClause : Clause
    {
        internal TextClause(int start, int end)
            : base(start, end) { }
    }

    internal class IfClause : Clause
    {
        internal IfClause(int start, int end,
            Expression test)
            : base(start, end)
        {
            Test = test;
        }

        internal Expression Test { get; }
    }

    internal class ElseIfClause : Clause
    {
        internal ElseIfClause(int start, int end,
            Expression test)
            : base(start, end)
        {
            Test = test;
        }

        internal Expression Test { get; }
    }

    internal class ElseClause : Clause
    {
        internal ElseClause(int start, int end)
            : base(start, end) { }
    }

    internal class EndIfClause : Clause
    {
        internal EndIfClause(int start, int end)
            : base(start, end) { }
    }

    internal class ForClause : Clause
    {
        internal ForClause(int start, int end,
            Expression loopVariable, Expression iterator)
            : base(start, end)
        {
            LoopVariable = loopVariable;
            Iterator = iterator;
        }

        internal Expression LoopVariable { get; }

        internal Expression Iterator { get; }
    }

    internal class EndForClause : Clause
    {
        internal EndForClause(int start, int end)
            : base(start, end) { }
    }

    internal class ExpressionClause : Clause
    {
        internal ExpressionClause(int start, int end,
            Expression expression)
            : base(start, end)
        {
            Expression = expression;
        }

        internal Expression Expression { get; }
    }
    #endregion Clause

    #region Expression
    internal enum UnaryOperator
    {
        Not,       // not A
        Positive,  //   + A
        Negative,  //   - A
    }

    internal enum BinaryOperator
    {
        Pipe,            // A | B
        FunctionCall,    // A ( B )
        Subscript,       // A [ B ]
        MemberAccess,    // A . B

        Or,              // A or B
        And,             // A and B

        Add,             // A + B
        Substract,       // A - B
        Multiply,        // A * B
        DivideFloat,     // A / B
        DivideInteger,   // A // B
        Modulo,          // A % B

        Less,            // A < B
        LessOrEqual,     // A <= B
        Equal,           // A == B
        GreaterOrEqual,  // A >= B
        Greater,         // A > B
        NotEqual,        // A != B
    }

    internal enum ValueType
    {
        String,
        Integer,
        Float,
        Boolean,
        Object,
    }

    internal abstract class Expression : SyntaxNode
    {
        internal protected Expression(int start, int end)
            : base(start, end) { }
    }

    internal class UnaryExpression : Expression
    {
        internal UnaryExpression(int start, int end,
            UnaryOperator @operator, Expression expression)
            : base(start, end)
        {
            Operator = @operator;
            Expression = expression;
        }

        internal UnaryOperator Operator { get; }

        internal Expression Expression { get; }
    }

    internal class BinaryExpression : Expression
    {
        internal BinaryExpression(int start, int end,
            BinaryOperator @operator, Expression left, Expression right)
            : base(start, end)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        internal BinaryOperator Operator { get; }

        internal Expression Left { get; }

        internal Expression Right { get; }
    }

    internal class ListExpression : Expression
    {
        internal ListExpression(int start, int end,
            IEnumerable<Expression> expressions)
            : base(start, end)
        {
            Expressions = expressions;
        }

        internal IEnumerable<Expression> Expressions { get; }
    }

    internal class ParenthesisExpression : Expression
    {
        internal ParenthesisExpression(int start, int end,
            Expression expression)
            : base(start, end)
        {
            Expression = expression;
        }

        internal Expression Expression { get; }
    }

    internal class SymbolExpression : Expression
    {
        internal SymbolExpression(int start, int end,
            string symbol)
            : base(start, end)
        {
            Symbol = symbol;
        }

        internal string Symbol { get; }
    }

    internal class LiteralExpression : Expression
    {
        internal LiteralExpression(int start, int end,
            ValueType type,
            string stringValue,
            long integerValue,
            double floatValue,
            bool booleanValue)
            : base(start, end)
        {
            ValueType = type;
            StringValue = stringValue;
            IntegerValue = integerValue;
            FloatValue = floatValue;
            BooleanValue = booleanValue;
        }

        internal ValueType ValueType { get; }

        internal string StringValue { get; }

        internal long IntegerValue { get; }

        internal double FloatValue { get; }

        internal bool BooleanValue { get; }
    }
    #endregion Expression

    #region Token
    internal enum TokenType
    {
        Invalid,

        StatementBegin,      // {%
        StatementEnd,        // %}
        ExpressionBegin,     // {{
        ExpressionEnd,       // }}

        If,                  // if
        ElseIf,              // elif
        Else,                // else
        EndIf,               // endif

        For,                 // for
        In,                  // in
        EndFor,              // 'endfor

        Not,                 // not
        Plus,                // +
        Minus,               // -

        Pipe,                // |
        LeftParenthesis,     // (
        RightParenthesis,    // )
        LeftSquareBracket,   // [
        RightSquareBracket,  // ]
        Dot,                 // .
        Comma,               // ,

        Or,                  // or
        And,                 // and
        Star,                // *
        Slash,               // /
        DoubleSlash,         // //
        Percent,             // %
        Less,                // <
        LessEqual,           // <=
        DoubleEqual,         // ==
        GreaterEqual,        // >=
        Greater,             // >
        ExclamationEqual,    // !=

        Symbol,              // \w+
        String,              // ' \w+ '
        Integer,             // \d+
        Float,               // (\.\d+)|)(\d+)\.(\d+)
        True,                // true
        False,               // false
    }

    internal struct Token
    {
        internal static Token Invalid(int start)
            => new Token(start, start, TokenType.Invalid);

        internal Token(int start, int end, TokenType tokenType)
        {
            Start = start;
            End = end;
            TokenType = tokenType;
        }

        internal int Start { get; }

        internal int End { get; }

        internal TokenType TokenType { get; }

        internal bool IsInvalid => TokenType == TokenType.Invalid;

        internal bool IsCompareOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Less:
                    case TokenType.LessEqual:
                    case TokenType.DoubleEqual:
                    case TokenType.GreaterEqual:
                    case TokenType.Greater:
                    case TokenType.ExclamationEqual:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal bool IsTermOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Plus:
                    case TokenType.Minus:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal bool IsFactorOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Star:
                    case TokenType.Slash:
                    case TokenType.DoubleSlash:
                    case TokenType.Percent:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal bool IsAtomExpressionOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Plus:
                    case TokenType.Minus:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal BinaryOperator AsBinaryOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Less: return BinaryOperator.Less;
                    case TokenType.LessEqual: return BinaryOperator.LessOrEqual;
                    case TokenType.DoubleEqual: return BinaryOperator.Equal;
                    case TokenType.GreaterEqual: return BinaryOperator.GreaterOrEqual;
                    case TokenType.Greater: return BinaryOperator.Greater;
                    case TokenType.ExclamationEqual: return BinaryOperator.NotEqual;

                    case TokenType.Plus: return BinaryOperator.Add;
                    case TokenType.Minus: return BinaryOperator.Substract;

                    case TokenType.Star: return BinaryOperator.Multiply;
                    case TokenType.Slash: return BinaryOperator.DivideFloat;
                    case TokenType.DoubleSlash: return BinaryOperator.DivideInteger;
                    case TokenType.Percent: return BinaryOperator.Modulo;

                    default:
                        throw new InvalidOperationException("Cannot convert to binary operator");
                }
            }
        }

        internal UnaryOperator AsUnaryOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Plus: return UnaryOperator.Positive;
                    case TokenType.Minus: return UnaryOperator.Negative;

                    default:
                        throw new InvalidOperationException("Cannot convert to unary operator");
                }
            }
        }
    }
    #endregion Token
}
