using System.Collections.Generic;
using Xunit;
using static SimpleJinja2DotNet.Tests.RendererTestUtils;

namespace SimpleJinja2DotNet.Tests
{
    public class RendererTest_Expression
    {
        [Theory]

        // Signle literal
        [InlineData("{{'abc'}}", "abc")]
        [InlineData("{{1}}", "1")]
        [InlineData("{{1.25}}", "1.25")]
        [InlineData("{{true}}", "True")]
        [InlineData("{{false}}", "False")]
        [InlineData("abc{{'def'}}", "abcdef")]
        [InlineData("{{'abc'}}def", "abcdef")]
        [InlineData("a{{1}}b", "a1b")]

        // Parenthesis
        [InlineData("{{('abc')}}", "abc")]
        [InlineData("{{(1)}}", "1")]
        [InlineData("{{(1.25)}}", "1.25")]
        [InlineData("{{(true)}}", "True")]
        [InlineData("{{(false)}}", "False")]
        [InlineData("abc{{('def')}}", "abcdef")]
        [InlineData("{{('abc')}}def", "abcdef")]
        [InlineData("a{{(1)}}b", "a1b")]

        // Unary
        [InlineData("{{not true}}", "False")]
        [InlineData("{{not false}}", "True")]
        [InlineData("{{not not true}}", "True")]
        [InlineData("{{not not false}}", "False")]
        [InlineData("{{not not not true}}", "False")]
        [InlineData("{{not not not false}}", "True")]

        [InlineData("{{not 'x'}}", "False")]
        [InlineData("{{not ''}}", "True")]
        [InlineData("{{not 1}}", "False")]
        [InlineData("{{not 0}}", "True")]

        [InlineData("{{+1}}", "1")]
        [InlineData("{{+1.25}}", "1.25")]

        [InlineData("{{-1}}", "-1")]
        [InlineData("{{-1.25}}", "-1.25")]

        // Binary
        [InlineData("{{'abc'|replace('a', 'x')}}", "xbc")]
        [InlineData("{{'abc'|replace('a', 'x')|replace('b', 'y')|replace('c', 'z')}}", "xyz")]

        [InlineData("{{true or true}}", "True")]
        [InlineData("{{true or false}}", "True")]
        [InlineData("{{false or true}}", "True")]
        [InlineData("{{false or false}}", "False")]

        [InlineData("{{true and true}}", "True")]
        [InlineData("{{true and false}}", "False")]
        [InlineData("{{false and true}}", "False")]
        [InlineData("{{false and false}}", "False")]

        [InlineData("{{'a'+'bc'}}", "abc")]
        [InlineData("{{'a'+1}}", "a1")]
        [InlineData("{{'a'+1.25}}", "a1.25")]
        [InlineData("{{'a '+true}}", "a True")]
        [InlineData("{{1+'a'}}", "1a")]
        [InlineData("{{1+2}}", "3")]
        [InlineData("{{1.25+'a'}}", "1.25a")]
        [InlineData("{{1.25+1}}", "2.25")]
        [InlineData("{{1.25+2.25}}", "3.5")]
        [InlineData("{{false+' a'}}", "False a")]

        [InlineData("{{1-2}}", "-1")]
        [InlineData("{{1-0.25}}", "0.75")]
        [InlineData("{{2.25-1}}", "1.25")]
        [InlineData("{{2.25-1.125}}", "1.125")]

        [InlineData("{{1*2}}", "2")]
        [InlineData("{{2*0.25}}", "0.5")]
        [InlineData("{{0.25*2}}", "0.5")]
        [InlineData("{{.5*.5}}", "0.25")]

        [InlineData("{{1/2}}", "0.5")]
        [InlineData("{{2/0.5}}", "4")]
        [InlineData("{{0.5/2}}", "0.25")]
        [InlineData("{{0.25/.5}}", "0.5")]

        [InlineData("{{1//2}}", "0")]
        [InlineData("{{3//2}}", "1")]

        [InlineData("{{5%2}}", "1")]
        [InlineData("{{7%4}}", "3")]

        [InlineData("{{'a'<'abc'}}", "True")]
        [InlineData("{{'a'<'b'}}", "True")]
        [InlineData("{{1<2}}", "True")]
        [InlineData("{{1<1}}", "False")]
        [InlineData("{{1<1.1}}", "True")]
        [InlineData("{{1.1<1}}", "False")]
        [InlineData("{{1.1<1.01}}", "False")]

        [InlineData("{{'a'<='abc'}}", "True")]
        [InlineData("{{'a'<='b'}}", "True")]
        [InlineData("{{1<=2}}", "True")]
        [InlineData("{{1<=1}}", "True")]
        [InlineData("{{1<=1.1}}", "True")]
        [InlineData("{{1.1<=1}}", "False")]
        [InlineData("{{1.123<=1.123}}", "True")]

        [InlineData("{{'a'>'abc'}}", "False")]
        [InlineData("{{'a'>'b'}}", "False")]
        [InlineData("{{1>2}}", "False")]
        [InlineData("{{1>1}}", "False")]
        [InlineData("{{1>1.1}}", "False")]
        [InlineData("{{1.1>1}}", "True")]
        [InlineData("{{1.1>1.01}}", "True")]

        [InlineData("{{'a'>='abc'}}", "False")]
        [InlineData("{{'a'>='b'}}", "False")]
        [InlineData("{{1>=2}}", "False")]
        [InlineData("{{1>=1}}", "True")]
        [InlineData("{{1>=1.1}}", "False")]
        [InlineData("{{1.1>=1}}", "True")]
        [InlineData("{{1.123>=1.123}}", "True")]

        [InlineData("{{'a'=='a'}}", "True")]
        [InlineData("{{1==1}}", "True")]
        [InlineData("{{1.123==1.123}}", "True")]
        [InlineData("{{0==0.0}}", "True")]
        [InlineData("{{1.0==1}}", "True")]
        [InlineData("{{true==true}}", "True")]
        [InlineData("{{true==false}}", "False")]
        [InlineData("{{false==true}}", "False")]
        [InlineData("{{false==false}}", "True")]

        [InlineData("{{'a'!='a'}}", "False")]
        [InlineData("{{1!=2}}", "True")]
        [InlineData("{{1.123!=1.123}}", "False")]
        [InlineData("{{0!=0.0}}", "False")]
        [InlineData("{{1.0!=1}}", "False")]
        [InlineData("{{true!=true}}", "False")]
        [InlineData("{{true!=false}}", "True")]
        [InlineData("{{false!=true}}", "True")]
        [InlineData("{{false!=false}}", "False")]

        // Combined
        [InlineData("{{3+2*2}}", "7")]
        [InlineData("{{(3+2)*2}}", "10")]

        public void RendererTest_Literal(string template, string expected)
        {
            RunTemplate(template, null, expected);
        }

        [Fact]
        public void RendererTest_Symbols()
        {
            // Single symbol
            RunTemplate(@"{{ v }}", new { v = "abc" }, @"abc");
            RunTemplate(@"{{ v }}", new { v = 123 }, @"123");
            RunTemplate(@"{{ v }}", new { v = 123L }, @"123");
            RunTemplate(@"{{ v }}", new { v = 1.25F }, @"1.25");
            RunTemplate(@"{{ v }}", new { v = 1.25D }, @"1.25");
            RunTemplate(@"{{ v }}", new { v = true }, @"True");
            RunTemplate(@"{{ v }}", new { v = false }, @"False");
            RunTemplate(@"{{ v }}", new { v = new object() }, @"System.Object");
            RunTemplate(@"{{ v }}", new { v = new TestClass() }, @"This is a custom object");
            RunTemplate(@"{{ v }}", new { v = (object)null }, @"");

            // Parenthesis
            RunTemplate(@"{{ (v) }}", new { v = "abc" }, @"abc");
            RunTemplate(@"{{ (v) }}", new { v = 123 }, @"123");
            RunTemplate(@"{{ (v) }}", new { v = 123L }, @"123");
            RunTemplate(@"{{ (v) }}", new { v = 1.25F }, @"1.25");
            RunTemplate(@"{{ (v) }}", new { v = 1.25D }, @"1.25");
            RunTemplate(@"{{ (v) }}", new { v = true }, @"True");
            RunTemplate(@"{{ (v) }}", new { v = false }, @"False");
            RunTemplate(@"{{ (v) }}", new { v = new object() }, @"System.Object");
            RunTemplate(@"{{ (v) }}", new { v = new TestClass() }, @"This is a custom object");
            RunTemplate(@"{{ (v) }}", new { v = (object)null }, @"");

            // Unary
            RunTemplate("{{not v}}", new { v = true }, "False");
            RunTemplate("{{not v}}", new { v = false }, "True");
            RunTemplate("{{not not v}}", new { v = true }, "True");
            RunTemplate("{{not not v}}", new { v = false }, "False");
            RunTemplate("{{not not not v}}", new { v = true }, "False");
            RunTemplate("{{not not not v}}", new { v = false }, "True");

            RunTemplate("{{not v}}", new { v = "x" }, "False");
            RunTemplate("{{not v}}", new { v = "" }, "True");
            RunTemplate("{{not v}}", new { v = 1 }, "False");
            RunTemplate("{{not v}}", new { v = 0 }, "True");

            RunTemplate("{{+v}}", new { v = 1 }, "1");
            RunTemplate("{{+v}}", new { v = 1.25 }, "1.25");

            RunTemplate("{{-v}}", new { v = 1 }, "-1");
            RunTemplate("{{-v}}", new { v = 1.25 }, "-1.25");

            // Binary
            RunTemplate("{{v|replace('a', 'x')}}", new { v = "abc" }, "xbc");
            RunTemplate(
                "{{v|replace('a', 'x')|replace('b', 'y')|replace('c', 'z')}}",
                new { v = "abc" },
                "xyz");
            RunTemplate(
                "{{v|replace(a.a, a.x)|replace(a.b, a.y)}}",
                new
                {
                    v = "abc",
                    a = new { a = "a", b = "b", x = "X", y = "Y" }
                },
                "XYc");

            RunTemplate(
                "{{ v[2] }}",
                new
                {
                    v = new[] { 0, 1, 2 }
                },
                "2");

            RunTemplate(
                "{{ v['x'] }}",
                new
                {
                    v = new Dictionary<string, int>
                    {
                        { "x", 5 }
                    }
                },
                "5");

            RunTemplate(
                "{{ v.x }}",
                new { v = new { x = 5 } },
                "5");

            RunTemplate(
                "{{ v.x.y.z }}",
                new { v = new { x = new { y = new { z = 5 } } } },
                "5");

            RunTemplate("{{a or b}}", new { a = false, b = true }, "True");
            RunTemplate("{{a and b}}", new { a = true, b = true }, "True");

            RunTemplate("{{a+b}}", new { a = "hi,", b = new TestClass() }, "hi,This is a custom object");
            RunTemplate("{{a+b}}", new { a = new TestClass(), b = "!" }, "This is a custom object!");

            // Combined
            RunTemplate("{{a.b!=''}}", new { a = new { b = "" } }, "False");
            RunTemplate("{{a.b!=''}}", new { a = new { b = "x" } }, "True");
            RunTemplate("{{a.b=='ABC'}}", new { a = new { b = "" } }, "False");
            RunTemplate("{{a.b=='ABC'}}", new { a = new { b = "ABC" } }, "True");
        }
    }

    internal class TestClass
    {
        public override string ToString()
            => "This is a custom object";
    }
}
