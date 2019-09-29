using System.Linq;
using Xunit;

namespace SimpleJinja2DotNet.Tests
{
    public class ParserTest_Blocks
    {
        [Fact]
        public void ParserTest_Empty()
        {
            var template = "";
            var parser = new Parser(template);
            var blockList = parser.Parse();

            Assert.Empty(blockList);
        }

        [Fact]
        public void ParserTest_Text()
        {
            var template = "abc";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (TextBlock)blockList[0];
            AssertTextBlock(b0, 0, 3, "abc");
        }

        [Fact]
        public void ParserTest_Text2()
        {
            var template = "%}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (TextBlock)blockList[0];
            AssertTextBlock(b0, 0, 2, "%}");
        }

        [Fact]
        public void ParserTest_Text3()
        {
            var template = "}}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (TextBlock)blockList[0];
            AssertTextBlock(b0, 0, 2, "}}");
        }

        [Fact]
        public void ParserTest_long()
        {
            var template = "abc{def{ghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ}}}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (TextBlock)blockList[0];
            AssertTextBlock(b0, 0, template.Length, template);
        }

        [Fact]
        public void ParserTest_If()
        {
            var template = "abc{% if a > b %} {% endif %}def";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Equal(3, blockList.Length);

            var b0 = (TextBlock)blockList[0];
            AssertTextBlock(b0, 0, 3, "abc");

            var b1 = (IfStatementBlock)blockList[1];
            AssertRange(b1, 3, 29);

            var testList = b1.TestList.ToArray();
            Assert.Single(testList);

            var t0 = testList[0];
            Assert.Empty(t0.Body);

            var e0 = (BinaryExpression)t0.Test;
            Assert.Equal(BinaryOperator.Greater, e0.Operator);
            Assert.Equal("a", ((SymbolExpression)e0.Left).Symbol);
            Assert.Equal("b", ((SymbolExpression)e0.Right).Symbol);

            var b2 = (TextBlock)blockList[2];
            AssertTextBlock(b2, 29, 32, "def");
        }

        [Fact]
        public void ParserTest_IfElse()
        {
            var template = "{% if true %} a {% else %} b {% endif %}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (IfStatementBlock)blockList[0];
            AssertRange(b0, 0, 40);

            var testList = b0.TestList.ToArray();
            Assert.Equal(2, testList.Length);

            var t0 = testList[0];
            AssertRange(t0, 0, 15);
            Assert.NotNull(t0.Test);
            var t0e0 = (LiteralExpression)t0.Test;
            Assert.Equal(ValueType.Boolean, t0e0.ValueType);
            Assert.True(t0e0.BooleanValue);
            var t0b0 = t0.Body.Single();
            Assert.True(t0b0 is TextBlock);
            Assert.Equal(" a", ((TextBlock)t0b0).Content);

            var t1 = testList[1];
            AssertRange(t1, 16, 28);
            Assert.Null(t1.Test);
            var t1b0 = t1.Body.Single();
            Assert.True(t1b0 is TextBlock);
            Assert.Equal(" b", ((TextBlock)t1b0).Content);
        }

        [Fact]
        public void ParserTest_IfElseIf()
        {
            var template = "{% if false %} a {% elif 1 %} b {% endif %}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (IfStatementBlock)blockList[0];
            AssertRange(b0, 0, 43);

            var testList = b0.TestList.ToArray();
            Assert.Equal(2, testList.Length);

            var t0 = testList[0];
            AssertRange(t0, 0, 16);
            Assert.NotNull(t0.Test);
            Assert.Single(t0.Body);
            var t0e0 = (LiteralExpression)t0.Test;
            Assert.Equal(ValueType.Boolean, t0e0.ValueType);
            Assert.False(t0e0.BooleanValue);
            var t0b0 = t0.Body.Single();
            Assert.True(t0b0 is TextBlock);
            Assert.Equal(" a", ((TextBlock)t0b0).Content);

            var t1 = testList[1];
            AssertRange(t1, 17, 31);
            Assert.NotNull(t1.Test);
            Assert.Single(t1.Body);
            var t1e0 = (LiteralExpression)t1.Test;
            Assert.Equal(ValueType.Integer, t1e0.ValueType);
            Assert.True(t1e0.BooleanValue);
            var t1b0 = t1.Body.Single();
            Assert.True(t1b0 is TextBlock);
            Assert.Equal(" b", ((TextBlock)t1b0).Content);
        }

        [Fact]
        public void ParserTest_IfElseIfElse()
        {
            var template = "{% if 'a' %} a {% elif 123 %} b {%else%} c {% endif %}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (IfStatementBlock)blockList[0];
            AssertRange(b0, 0, 54);

            var testList = b0.TestList.ToArray();
            Assert.Equal(3, testList.Length);

            var t0 = testList[0];
            AssertRange(t0, 0, 14);
            Assert.NotNull(t0.Test);
            Assert.Single(t0.Body);
            var t0e0 = (LiteralExpression)t0.Test;
            Assert.Equal(ValueType.String, t0e0.ValueType);
            Assert.True(t0e0.BooleanValue);
            Assert.Equal("a", t0e0.StringValue);
            var t0b0 = t0.Body.Single();
            Assert.True(t0b0 is TextBlock);
            Assert.Equal(" a", ((TextBlock)t0b0).Content);

            var t1 = testList[1];
            AssertRange(t1, 15, 31);
            Assert.NotNull(t1.Test);
            Assert.Single(t1.Body);
            var t1e0 = (LiteralExpression)t1.Test;
            Assert.Equal(ValueType.Integer, t1e0.ValueType);
            Assert.True(t1e0.BooleanValue);
            Assert.Equal(123, t1e0.IntegerValue);
            var t1b0 = t1.Body.Single();
            Assert.True(t1b0 is TextBlock);
            Assert.Equal(" b", ((TextBlock)t1b0).Content);

            var t2 = testList[2];
            AssertRange(t2, 32, 42);
            Assert.Null(t2.Test);
            var t2b0 = t2.Body.Single();
            Assert.True(t2b0 is TextBlock);
            Assert.Equal(" c", ((TextBlock)t2b0).Content);
        }

        [Fact]
        public void ParserTest_For_Empty()
        {
            var template = "{%for a in b%}{%endfor%}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (ForStatementBlock)blockList[0];
            var e0 = (SymbolExpression)b0.LoopVariable;
            Assert.Equal("a", e0.Symbol);
            var e1 = (SymbolExpression)b0.Iterator;
            Assert.Equal("b", e1.Symbol);
        }

        [Fact]
        public void ParserTest_For()
        {
            var template = "{%for a in b%}<A>{%endfor%}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (ForStatementBlock)blockList[0];
            Assert.Single(b0.Body);

            var b0b0 = (TextBlock)b0.Body.Single();
            AssertTextBlock(b0b0, 14, 17, "<A>");
        }

        [Fact]
        public void ParserTest_EmbeddedIf()
        {
            var template =
                "{% if a %}" +
                "  <a>" +
                "  {% if b %}" +
                "    <b>" +
                "  {% endif %}" +
                "{% elif c %}" +
                "  <c>" +
                "  {% if d %}" +
                "    <d>" +
                "  {% else %}" +
                "    <e>" +
                "  {% endif %}" +
                "  <f>" +
                "{% endif %}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (IfStatementBlock)blockList[0];
            var b0Tests = b0.TestList.ToArray();
            Assert.Equal(2, b0Tests.Length);

            var b0t0 = b0Tests[0];
            Assert.Equal("a", ((SymbolExpression)b0t0.Test).Symbol);
            var b0t0Body = b0t0.Body.ToArray();
            Assert.Equal(2, b0t0Body.Length);
            var b0t0b0 = (TextBlock)b0t0Body[0];
            Assert.Equal("  <a>", b0t0b0.Content);
            var b0t0b1 = (IfStatementBlock)b0t0Body[1];
            var b0t0b1Tests = b0t0b1.TestList.ToArray();
            Assert.Single(b0t0b1Tests);
            var b0t0b1t0 = b0t0b1Tests[0];
            Assert.Equal("b", ((SymbolExpression)b0t0b1t0.Test).Symbol);
            Assert.Equal("    <b>", ((TextBlock)b0t0b1t0.Body.Single()).Content);

            var b0t1 = b0Tests[1];
            Assert.Equal("c", ((SymbolExpression)b0t1.Test).Symbol);
            var b0t1Body = b0t1.Body.ToArray();
            Assert.Equal(3, b0t1Body.Length);
            var b0t1b0 = (TextBlock)b0t1Body[0];
            Assert.Equal("  <c>", b0t1b0.Content);
            var b0t1b1 = (IfStatementBlock)b0t1Body[1];
            var b0t1b1Tests = b0t1b1.TestList.ToArray();
            Assert.Equal(2, b0t1b1Tests.Length);
            var b0t1b2 = (TextBlock)b0t1Body[2];
            Assert.Equal("  <f>", b0t1b2.Content);
        }

        [Fact]
        public void ParserTest_EmbeddedFor()
        {
            var template =
                "{% for a in b %}" +
                "{% for c in d %}" +
                "<abc>" +
                "{% endfor %}" +
                "{% endfor %}";
            var parser = new Parser(template);
            var blockList = parser.Parse().ToArray();

            Assert.Single(blockList);

            var b0 = (ForStatementBlock)blockList[0];
            Assert.Equal("a", ((SymbolExpression)b0.LoopVariable).Symbol);
            Assert.Equal("b", ((SymbolExpression)b0.Iterator).Symbol);

            Assert.Single(b0.Body);

            var b0b0 = (ForStatementBlock)b0.Body.Single();
            Assert.Equal("c", ((SymbolExpression)b0b0.LoopVariable).Symbol);
            Assert.Equal("d", ((SymbolExpression)b0b0.Iterator).Symbol);
        }

        private void AssertRange(Block b, int start, int end)
        {
            Assert.Equal(start, b.Start);
            Assert.Equal(end, b.End);
        }

        private void AssertTextBlock(TextBlock b, int start, int end, string content)
        {
            AssertRange(b, start, end);
            Assert.Equal(content, b.Content);
        }
    }
}
