using System.Collections.Generic;
using Xunit;

namespace SimpleJinja2DotNet.Tests
{
    public class LexerTest
    {
        [Fact]
        public void LexerTest_IfStatement()
        {
            var tokens = GetTokens(@"{% if a > b %}");
            Assert.Equal(7, tokens.Count);
            AssertToken(tokens[0], 0, 2, TokenType.StatementBegin);
            AssertToken(tokens[1], 3, 5, TokenType.If);
            AssertToken(tokens[2], 6, 7, TokenType.Symbol);
            AssertToken(tokens[3], 8, 9, TokenType.Greater);
            AssertToken(tokens[4], 10, 11, TokenType.Symbol);
            AssertToken(tokens[5], 12, 14, TokenType.StatementEnd);
            Assert.True(tokens[6].IsInvalid);
        }

        [Fact]
        public void LexerTest_ElifStatement()
        {
            var tokens = GetTokens(@"{% elif a <= b %}");
            Assert.Equal(7, tokens.Count);
            AssertToken(tokens[0], 0, 2, TokenType.StatementBegin);
            AssertToken(tokens[1], 3, 7, TokenType.ElseIf);
            AssertToken(tokens[2], 8, 9, TokenType.Symbol);
            AssertToken(tokens[3], 10, 12, TokenType.LessEqual);
            AssertToken(tokens[4], 13, 14, TokenType.Symbol);
            AssertToken(tokens[5], 15, 17, TokenType.StatementEnd);
            Assert.True(tokens[6].IsInvalid);
        }

        [Fact]
        public void LexerTest_EndIfStatement()
        {
            var tokens = GetTokens(@"{% endif %}");
            Assert.Equal(4, tokens.Count);
            AssertToken(tokens[0], 0, 2, TokenType.StatementBegin);
            AssertToken(tokens[1], 3, 8, TokenType.EndIf);
            AssertToken(tokens[2], 9, 11, TokenType.StatementEnd);
            Assert.True(tokens[3].IsInvalid);
        }

        [Fact]
        public void LexerTest_String_Empty()
        {
            var tokens = GetTokens(@"''");
            Assert.Equal(2, tokens.Count);
            AssertToken(tokens[0], 0, 2, TokenType.String);
            Assert.True(tokens[1].IsInvalid);
        }

        [Fact]
        public void LexerTest_String_Simple()
        {
            var tokens = GetTokens(@"'abc'");
            Assert.Equal(2, tokens.Count);
            AssertToken(tokens[0], 0, 5, TokenType.String);
            Assert.True(tokens[1].IsInvalid);
        }

        [Fact]
        public void LexerTest_String_Escape()
        {
            var tokens = GetTokens(@"'a\r\nbc'");
            Assert.Equal(2, tokens.Count);
            AssertToken(tokens[0], 0, 9, TokenType.String);
            Assert.True(tokens[1].IsInvalid);
        }

        [Fact]
        public void LexerTest_String_ContainsKeywords()
        {
            var tokens = GetTokens(@"'abc {% if %}'");
            Assert.Equal(2, tokens.Count);
            AssertToken(tokens[0], 0, 14, TokenType.String);
            Assert.True(tokens[1].IsInvalid);
        }

        [Fact]
        public void LexerTest_Integer()
        {
            var tokens = GetTokens(@"123");
            Assert.Equal(2, tokens.Count);
            AssertToken(tokens[0], 0, 3, TokenType.Integer);
            Assert.True(tokens[1].IsInvalid);
        }

        [Fact]
        public void LexerTest_Integer_WithDot()
        {
            var tokens = GetTokens(@"123.");
            Assert.Equal(3, tokens.Count);
            AssertToken(tokens[0], 0, 3, TokenType.Integer);
            AssertToken(tokens[1], 3, 4, TokenType.Dot);
            Assert.True(tokens[2].IsInvalid);
        }

        [Fact]
        public void LexerTest_Float()
        {
            var tokens = GetTokens(@"123.456");
            Assert.Equal(2, tokens.Count);
            AssertToken(tokens[0], 0, 7, TokenType.Float);
            Assert.True(tokens[1].IsInvalid);
        }

        [Fact]
        public void LexerTest_Float_DotStarted()
        {
            var tokens = GetTokens(@".123");
            Assert.Equal(2, tokens.Count);
            AssertToken(tokens[0], 0, 4, TokenType.Float);
            Assert.True(tokens[1].IsInvalid);
        }

        [Fact]
        public void LexerTest_Boolean()
        {
            var tokens = GetTokens(@"true false");
            Assert.Equal(3, tokens.Count);
            AssertToken(tokens[0], 0, 4, TokenType.True);
            AssertToken(tokens[1], 5, 10, TokenType.False);
            Assert.True(tokens[2].IsInvalid);
        }

        private IList<Token> GetTokens(string s)
        {
            var lex = new Lexer(s);
            var tokens = new List<Token>();

            Token t;
            var i = 0;
            do
            {
                t = lex.GetToken(i);
                tokens.Add(t);
                if (!t.IsInvalid)
                    i = t.End;
            } while (!t.IsInvalid);

            return tokens;
        }

        private void AssertToken(Token t, int start, int end, TokenType type)
        {
            Assert.Equal(start, t.Start);
            Assert.Equal(end, t.End);
            Assert.Equal(type, t.TokenType);
        }
    }
}
