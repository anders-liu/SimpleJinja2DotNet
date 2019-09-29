using System.Linq;
using Xunit;

namespace SimpleJinja2DotNet.Tests
{
    public class ParserTest_Expressions
    {
        [Fact]
        public void ParserTest_Filter_NoArg()
        {
            var template = "{{a|b}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 5);
            Assert.Equal(BinaryOperator.Pipe, e.Operator);

            var left = (SymbolExpression)e.Left;
            AssertRange(left, 2, 3);
            Assert.Equal("a", left.Symbol);

            var right = (SymbolExpression)e.Right;
            AssertRange(right, 4, 5);
            Assert.Equal("b", right.Symbol);
        }

        [Fact]
        public void ParserTest_Filter_1Arg()
        {
            var template = "{{a|b(1)}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 8);
            Assert.Equal(BinaryOperator.Pipe, e.Operator);

            var left = (SymbolExpression)e.Left;
            AssertRange(left, 2, 3);
            Assert.Equal("a", left.Symbol);

            var right = (BinaryExpression)e.Right;
            AssertRange(right, 4, 8);
            Assert.Equal(BinaryOperator.FunctionCall, right.Operator);

            var rightLeft = (SymbolExpression)right.Left;
            AssertRange(rightLeft, 4, 5);
            Assert.Equal("b", rightLeft.Symbol);

            var rightRight = (ListExpression)right.Right;
            AssertRange(rightRight, 5, 8);
            Assert.Single(rightRight.Expressions);

            var arg0 = (LiteralExpression)rightRight.Expressions.Single();
            Assert.Equal(ValueType.Integer, arg0.ValueType);
            Assert.Equal(1, arg0.IntegerValue);
        }

        [Fact]
        public void ParserTest_Filter_2Args()
        {
            var template = "{{ a | b('x', 'y') }}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 3, 18);
            Assert.Equal(BinaryOperator.Pipe, e.Operator);

            var right = (BinaryExpression)e.Right;
            AssertRange(right, 7, 18);
            Assert.Equal(BinaryOperator.FunctionCall, right.Operator);

            var rightRight = (ListExpression)right.Right;
            AssertRange(rightRight, 8, 18);

            var args = rightRight.Expressions.ToArray();
            Assert.Equal(2, args.Length);

            var arg0 = (LiteralExpression)args[0];
            AssertRange(arg0, 9, 12);
            Assert.Equal(ValueType.String, arg0.ValueType);
            Assert.Equal("x", arg0.StringValue);

            var arg1 = (LiteralExpression)args[1];
            AssertRange(arg1, 14, 17);
            Assert.Equal(ValueType.String, arg1.ValueType);
            Assert.Equal("y", arg1.StringValue);
        }

        [Fact]
        public void ParserTest_Filter_Multiple()
        {
            var template = "{{a|b('x','y')|c('v','w')}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 25);
            Assert.Equal(BinaryOperator.Pipe, e.Operator);

            var left = (BinaryExpression)e.Left;
            AssertRange(left, 2, 14);
            Assert.Equal(BinaryOperator.Pipe, left.Operator);

            var leftLeft = (SymbolExpression)left.Left;
            AssertRange(leftLeft, 2, 3);
            Assert.Equal("a", leftLeft.Symbol);

            var leftRight = (BinaryExpression)left.Right;
            AssertRange(leftRight, 4, 14);
            Assert.Equal(BinaryOperator.FunctionCall, leftRight.Operator);

            var leftRightLeft = (SymbolExpression)leftRight.Left;
            AssertRange(leftRightLeft, 4, 5);
            Assert.Equal("b", leftRightLeft.Symbol);

            var leftRightRight = (ListExpression)leftRight.Right;
            AssertRange(leftRightRight, 5, 14);
            Assert.Equal(2, leftRightRight.Expressions.Count());

            var right = (BinaryExpression)e.Right;
            AssertRange(right, 15, 25);
            Assert.Equal(BinaryOperator.FunctionCall, right.Operator);

            var rightLeft = (SymbolExpression)right.Left;
            AssertRange(rightLeft, 15, 16);
            Assert.Equal("c", rightLeft.Symbol);

            var rightRight = (ListExpression)right.Right;
            AssertRange(rightRight, 16, 25);
            Assert.Equal(2, rightRight.Expressions.Count());
        }

        [Fact]
        public void ParserTest_OrTest()
        {
            var template = "{{a or b}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 8);
            Assert.Equal(BinaryOperator.Or, e.Operator);

            var left = (SymbolExpression)e.Left;
            AssertRange(left, 2, 3);
            Assert.Equal("a", left.Symbol);

            var right = (SymbolExpression)e.Right;
            AssertRange(right, 7, 8);
            Assert.Equal("b", right.Symbol);
        }

        [Fact]
        public void ParserTest_OrOrTest()
        {
            var template = "{{a or b or c}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            Assert.Equal(BinaryOperator.Or, e.Operator);

            var left = (BinaryExpression)e.Left;
            Assert.Equal(BinaryOperator.Or, left.Operator);

            var leftLeft = (SymbolExpression)left.Left;
            Assert.Equal("a", leftLeft.Symbol);

            var leftRight = (SymbolExpression)left.Right;
            Assert.Equal("b", leftRight.Symbol);

            var right = (SymbolExpression)e.Right;
            Assert.Equal("c", right.Symbol);
        }

        [Fact]
        public void ParserTest_AndTest()
        {
            var template = "{{a and b}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 9);
            Assert.Equal(BinaryOperator.And, e.Operator);

            var left = (SymbolExpression)e.Left;
            AssertRange(left, 2, 3);
            Assert.Equal("a", left.Symbol);

            var right = (SymbolExpression)e.Right;
            AssertRange(right, 8, 9);
            Assert.Equal("b", right.Symbol);
        }

        [Fact]
        public void ParserTest_AndAndTest()
        {
            var template = "{{a and b and c}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            Assert.Equal(BinaryOperator.And, e.Operator);

            var left = (BinaryExpression)e.Left;
            Assert.Equal(BinaryOperator.And, left.Operator);

            var leftLeft = (SymbolExpression)left.Left;
            Assert.Equal("a", leftLeft.Symbol);

            var leftRight = (SymbolExpression)left.Right;
            Assert.Equal("b", leftRight.Symbol);

            var right = (SymbolExpression)e.Right;
            Assert.Equal("c", right.Symbol);
        }

        [Fact]
        public void ParserTest_NotTest()
        {
            var template = "{{not a}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (UnaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 7);
            Assert.Equal(UnaryOperator.Not, e.Operator);

            var e0 = (SymbolExpression)e.Expression;
            AssertRange(e0, 6, 7);
            Assert.Equal("a", e0.Symbol);
        }

        [Fact]
        public void ParserTest_NotNotTest()
        {
            var template = "{{not not a}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (UnaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            Assert.Equal(UnaryOperator.Not, e.Operator);

            var e0 = (UnaryExpression)e.Expression;
            Assert.Equal(UnaryOperator.Not, e0.Operator);

            var e1 = (SymbolExpression)e0.Expression;
            Assert.Equal("a", e1.Symbol);
        }

        [Fact]
        public void ParserTest_Compare()
        {
            var template = "{{a == ''}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 9);
            Assert.Equal(BinaryOperator.Equal, e.Operator);

            var left = (SymbolExpression)e.Left;
            AssertRange(left, 2, 3);
            Assert.Equal("a", left.Symbol);

            var right = (LiteralExpression)e.Right;
            AssertRange(right, 7, 9);
            Assert.Equal(ValueType.String, right.ValueType);
            Assert.Equal("", right.StringValue);
        }

        [Fact]
        public void ParserTest_Math()
        {
            var template = "{{1.5 - 3.125}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 13);
            Assert.Equal(BinaryOperator.Substract, e.Operator);

            var left = (LiteralExpression)e.Left;
            AssertRange(left, 2, 5);
            Assert.Equal(1.5, left.FloatValue);

            var right = (LiteralExpression)e.Right;
            AssertRange(right, 8, 13);
            Assert.Equal(ValueType.Float, right.ValueType);
            Assert.Equal(3.125, right.FloatValue);
        }

        [Fact]
        public void ParserTest_Term()
        {
            var template = "{{a // 1}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 8);
            Assert.Equal(BinaryOperator.DivideInteger, e.Operator);

            var left = (SymbolExpression)e.Left;
            AssertRange(left, 2, 3);
            Assert.Equal("a", left.Symbol);

            var right = (LiteralExpression)e.Right;
            AssertRange(right, 7, 8);
            Assert.Equal(ValueType.Integer, right.ValueType);
            Assert.Equal(1, right.IntegerValue);
        }

        [Fact]
        public void ParserTest_Factor()
        {
            var template = "{{-1}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (UnaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 4);
            Assert.Equal(UnaryOperator.Negative, e.Operator);

            var e0 = (LiteralExpression)e.Expression;
            AssertRange(e0, 3, 4);
            Assert.Equal(1, e0.IntegerValue);
        }

        [Fact]
        public void ParserTest_AtomExpression_MemberAccess()
        {
            var template = "{{a.b}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 5);
            Assert.Equal(BinaryOperator.MemberAccess, e.Operator);

            var left = (SymbolExpression)e.Left;
            AssertRange(left, 2, 3);
            Assert.Equal("a", left.Symbol);

            var right = (SymbolExpression)e.Right;
            AssertRange(right, 4, 5);
            Assert.Equal("b", right.Symbol);
        }

        [Fact]
        public void ParserTest_AtomExpression_Subscript()
        {
            var template = "{{a[b]}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 6);
            Assert.Equal(BinaryOperator.Subscript, e.Operator);

            var left = (SymbolExpression)e.Left;
            AssertRange(left, 2, 3);
            Assert.Equal("a", left.Symbol);

            var right = (SymbolExpression)e.Right;
            AssertRange(right, 4, 5);
            Assert.Equal("b", right.Symbol);
        }

        [Fact]
        public void ParserTest_AtomExpression_Combined1()
        {
            var template = "{{a.b.c}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            Assert.Equal(BinaryOperator.MemberAccess, e.Operator);

            var left = (BinaryExpression)e.Left;
            Assert.Equal(BinaryOperator.MemberAccess, left.Operator);
            Assert.Equal("a", ((SymbolExpression)left.Left).Symbol);
            Assert.Equal("b", ((SymbolExpression)left.Right).Symbol);

            var right = (SymbolExpression)e.Right;
            Assert.Equal("c", right.Symbol);
        }

        [Fact]
        public void ParserTest_AtomExpression_Combined2()
        {
            var template = "{{a[b][c]}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            Assert.Equal(BinaryOperator.Subscript, e.Operator);

            var left = (BinaryExpression)e.Left;
            Assert.Equal(BinaryOperator.Subscript, left.Operator);
            Assert.Equal("a", ((SymbolExpression)left.Left).Symbol);
            Assert.Equal("b", ((SymbolExpression)left.Right).Symbol);

            var right = (SymbolExpression)e.Right;
            Assert.Equal("c", right.Symbol);
        }

        [Fact]
        public void ParserTest_AtomExpression_Combined3()
        {
            var template = "{{a.b[c]}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            Assert.Equal(BinaryOperator.Subscript, e.Operator);

            var left = (BinaryExpression)e.Left;
            Assert.Equal(BinaryOperator.MemberAccess, left.Operator);
            Assert.Equal("a", ((SymbolExpression)left.Left).Symbol);
            Assert.Equal("b", ((SymbolExpression)left.Right).Symbol);

            var right = (SymbolExpression)e.Right;
            Assert.Equal("c", right.Symbol);
        }

        [Fact]
        public void ParserTest_AtomExpression_Combined4()
        {
            var template = "{{a[b].c}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (BinaryExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            Assert.Equal(BinaryOperator.MemberAccess, e.Operator);

            var left = (BinaryExpression)e.Left;
            Assert.Equal(BinaryOperator.Subscript, left.Operator);
            Assert.Equal("a", ((SymbolExpression)left.Left).Symbol);
            Assert.Equal("b", ((SymbolExpression)left.Right).Symbol);

            var right = (SymbolExpression)e.Right;
            Assert.Equal("c", right.Symbol);
        }

        [Fact]
        public void ParserTest_Parenthesis()
        {
            var template = "{{(a+b)}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (ParenthesisExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 7);

            var e0 = (BinaryExpression)e.Expression;
            AssertRange(e0, 3, 6);
            Assert.Equal(BinaryOperator.Add, e0.Operator);

            var left = (SymbolExpression)e0.Left;
            AssertRange(left, 3, 4);
            Assert.Equal("a", left.Symbol);

            var right = (SymbolExpression)e0.Right;
            AssertRange(right, 5, 6);
            Assert.Equal("b", right.Symbol);
        }

        [Fact]
        public void ParserTest_Parenthesis_Embedded()
        {
            var template = "{{(a*(b+c))}}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (ParenthesisExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 2, 11);

            var e0 = (BinaryExpression)e.Expression;
            AssertRange(e0, 3, 10);
            Assert.Equal(BinaryOperator.Multiply, e0.Operator);

            var left = (SymbolExpression)e0.Left;
            AssertRange(left, 3, 4);
            Assert.Equal("a", left.Symbol);

            var right = (ParenthesisExpression)e0.Right;
            AssertRange(right, 5, 10);

            var e2 = (BinaryExpression)right.Expression;
            AssertRange(e2, 6, 9);
            Assert.Equal(BinaryOperator.Add, e2.Operator);
            Assert.Equal("b", ((SymbolExpression)e2.Left).Symbol);
            Assert.Equal("c", ((SymbolExpression)e2.Right).Symbol);
        }

        [Fact]
        public void ParserTest_Symbol()
        {
            var template = "{{ abc }}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (SymbolExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 3, 6);
            Assert.Equal("abc", e.Symbol);
        }

        [Fact]
        public void ParserTest_Literal_String()
        {
            var template = "{{ '}}' }}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (LiteralExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 3, 7);
            Assert.Equal(ValueType.String, e.ValueType);
            Assert.Equal("}}", e.StringValue);
            Assert.Equal(0, e.IntegerValue);
            Assert.Equal(0.0, e.FloatValue);
            Assert.True(e.BooleanValue);
        }

        [Fact]
        public void ParserTest_Literal_Integer()
        {
            var template = "{{ 123 }}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (LiteralExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 3, 6);
            Assert.Equal(ValueType.Integer, e.ValueType);
            Assert.Equal("123", e.StringValue);
            Assert.Equal(123, e.IntegerValue);
            Assert.Equal(123.0, e.FloatValue);
            Assert.True(e.BooleanValue);
        }

        [Fact]
        public void ParserTest_Literal_Float()
        {
            var template = "{{ 123.125 }}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (LiteralExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 3, 10);
            Assert.Equal(ValueType.Float, e.ValueType);
            Assert.Equal("123.125", e.StringValue);
            Assert.Equal(123, e.IntegerValue);
            Assert.Equal(123.125, e.FloatValue);
            Assert.True(e.BooleanValue);
        }

        [Fact]
        public void ParserTest_Literal_True()
        {
            var template = "{{ true }}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (LiteralExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 3, 7);
            Assert.Equal(ValueType.Boolean, e.ValueType);
            Assert.Equal("true", e.StringValue);
            Assert.Equal(1, e.IntegerValue);
            Assert.Equal(1.0, e.FloatValue);
            Assert.True(e.BooleanValue);
        }

        [Fact]
        public void ParserTest_Literal_False()
        {
            var template = "{{ false }}";
            var parser = new Parser(template);
            var blockList = parser.Parse();
            var e = (LiteralExpression)
                ((ExpressionBlock)blockList.Single()).Expression;

            AssertRange(e, 3, 8);
            Assert.Equal(ValueType.Boolean, e.ValueType);
            Assert.Equal("false", e.StringValue);
            Assert.Equal(0, e.IntegerValue);
            Assert.Equal(0.0, e.FloatValue);
            Assert.False(e.BooleanValue);
        }

        private void AssertRange(Expression e, int start, int end)
        {
            Assert.Equal(start, e.Start);
            Assert.Equal(end, e.End);
        }
    }
}
