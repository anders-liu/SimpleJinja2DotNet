using Xunit;

namespace SimpleJinja2DotNet.Tests
{
    public class TextUtilsTest
    {
        [Theory]
        [InlineData("", 0, 1, 1)]
        [InlineData("abc", 0, 1, 1)]
        [InlineData("abc", 1, 1, 2)]
        [InlineData("abc", 3, 1, 4)]
        [InlineData("abc", 10, 1, 4)]
        [InlineData("abc\ndef", 4, 2, 1)]
        [InlineData("abc\ndef", 5, 2, 2)]
        [InlineData("abc\r\ndef", 4, 1, 4)]
        [InlineData("abc\r\ndef", 5, 2, 1)]
        [InlineData("a\nb\r\ncde", 6, 3, 2)]
        [InlineData("\n\rabc", 2, 3, 1)]
        [InlineData("\r\n\r\nabc", 4, 3, 1)]
        [InlineData("\r\n\r\nabc", 5, 3, 2)]
        public void TextUtilsTest_TextUtils(
            string s, int charIndex, int row, int column)
        {
            (var r, var c) = TextUtils.GetCharPosition(s, charIndex);
            Assert.Equal(row, r);
            Assert.Equal(column, c);
        }
    }
}
