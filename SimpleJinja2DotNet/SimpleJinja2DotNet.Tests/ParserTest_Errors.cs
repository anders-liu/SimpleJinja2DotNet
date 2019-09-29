using Xunit;

namespace SimpleJinja2DotNet.Tests
{
    public class ParserTest_Errors
    {
        [Theory]
        [InlineData(
            @"abc{%if def",
            1, 12, ParsingError.IncompletedStatementBlock)]
        [InlineData(
            "\r\n\r\nabc{{ def\r\n",
            3, 4, ParsingError.IncompletedExpressionBlock)]
        [InlineData(@"
<h1>
{% abc %}
</h1>",
            3, 4, ParsingError.UnknownStatementType)]
        [InlineData(
            @"{% if %}",
            1, 6, ParsingError.MissingTestExpression)]
        [InlineData(@"
{% if a %}
{% elif %}
{% endif %}",
               3, 8,
               ParsingError.MissingTestExpression)]
        [InlineData(
            @"{% for %}",
            1, 7, ParsingError.MissingLoopVariable)]
        [InlineData(
            @"{% for in %}",
            1, 7, ParsingError.MissingLoopVariable)]
        [InlineData(
            @"{% for a %}",
            1, 9, ParsingError.MissingInKeyword)]
        [InlineData(
            @"{% for a in %}",
            1, 12, ParsingError.MissingIterator)]
        [InlineData(@"{% if a %}",
            1, 1, ParsingError.MissingEndIf)]
        [InlineData(@"{% for a in b %}",
            1, 1, ParsingError.MissingEndFor)]
        [InlineData(@"{{ a | }}",
            1, 7, ParsingError.MissingFilterCall)]
        [InlineData(@"{{ a | 123}}",
            1, 8, ParsingError.InvalidFilter)]
        [InlineData(@"{{ a|b( }}",
            1, 8, ParsingError.MissingParenthesis)]
        [InlineData(@"{{ a+( }}",
            1, 7, ParsingError.MissingExpression)]
        [InlineData(@"{{ a[b }}",
            1, 7, ParsingError.MissingEndOfSubscript)]
        [InlineData(@"{{ a. 1 }}",
            1, 6, ParsingError.InvalidSymbol)]
        [InlineData(@"{{ 1234567890123456 }}",
            1, 4, ParsingError.InvalidNumber)]
        public void ParserTest_Throws(
            string template,
            int row,
            int column,
            ParsingError error)
        {
            var parser = new Parser(template);
            var ex = Assert.Throws<ParsingException>(
                () => parser.Parse());
            Assert.Equal(row, ex.Row);
            Assert.Equal(column, ex.Column);
            Assert.Equal(error, ex.Error);
        }
    }
}
