using System;
using Xunit;
using static SimpleJinja2DotNet.Tests.RendererTestUtils;

namespace SimpleJinja2DotNet.Tests
{
    public class RendererTest_Basic
    {
        [Fact]
        public void RendererTest_Empty()
        {
            var template = "";
            var globals = (object)null;
            var expected = "";
            RunTemplate(template, globals, expected);
        }

        [Fact]
        public void RendererTest_Text()
        {
            var template = "abc";
            var globals = (object)null;
            var expected = "abc";
            RunTemplate(template, globals, expected);
        }

        [Fact]
        public void RendererTest_Null()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => Template.FromString(null));
            Assert.Equal("str", ex.ParamName);
        }

        [Fact]
        public void RendererTest_If()
        {
            var template =
@"<html>
<title>{% if a.t != '' %}{{ a.t }}{% else %} hello {% endif %}</title>
</html>
";

            var resultNoTitle =
@"<html>
<title> hello</title>
</html>
";

            var resultEmptyTitle =
@"<html>
<title></title>
</html>
";

            var resultWithTitle =
@"<html>
<title>Anders</title>
</html>
";

            RunTemplate(template,
                new { a = new { t = "Anders" } },
                resultWithTitle);

            RunTemplate(template,
                null,
                resultNoTitle);

            RunTemplate(template,
                new { },
                resultNoTitle);

            RunTemplate(template,
                new { a = (object)null },
                resultNoTitle);

            RunTemplate(template,
                new { a = new { } },
                resultNoTitle);

            RunTemplate(template,
                new { a = new { t = "" } },
                resultNoTitle);

            RunTemplate(template,
                new { a = new { t = (object)null } },
                resultEmptyTitle);
        }

        [Fact]
        public void RendererTest_If_Embedded()
        {
            var template =
@"<html>
{% if (a > 3) %}
    {% if b < 3 %}
        <div>(a > 3) &amp; (b < 3)</div>
    {% elif b > 3 %}
        <div>(a > 3) &amp; (b > 3)</div>
    {% else %}
        <div>(a > 3) &amp; (b == 3)</div>
    {% endif %}
{% elif (s == 'def') %}
    {% for i in def %}
        {{i}}
    {% endfor %}
{% elif (s > 'abc') %}
    <div>{{x}}</div>
{% endif %}
</html>
";

            RunTemplate(template,
                new { a = 5, b = 1 },
@"<html>
        <div>(a > 3) &amp; (b < 3)</div>
</html>
");

            RunTemplate(template,
                new { a = 5, b = 5 },
@"<html>
        <div>(a > 3) &amp; (b > 3)</div>
</html>
");

            RunTemplate(template,
                new { a = 5, b = 3 },
@"<html>
        <div>(a > 3) &amp; (b == 3)</div>
</html>
");

            RunTemplate(template,
                new { a = 1, s = "def", def = new[] { 1, 2, 3 } },
@"<html>123
</html>
");

            RunTemplate(template,
                new { a = 1, s = "abcde", x = 123 },
@"<html>
    <div>123</div>
</html>
");
        }

        [Fact]
        public void RendererTest_For()
        {
            var template =
@"<html>
<ul>
    {% for i in items %}
    <li>{{i.text}}</li>
    {% endfor %}
</ul>
</html>
";

            var result1Item =
@"<html>
<ul>
    <li>aaa</li>
</ul>
</html>
";

            var result3Items =
@"<html>
<ul>
    <li>aaa</li>
    <li>bbb</li>
    <li>ccc</li>
</ul>
</html>
";

            var resultEmptyList =
@"<html>
<ul>
</ul>
</html>
";

            RunTemplate(template,
                new
                {
                    items = new[] { new { text = "aaa" }, }
                },
                result1Item);

            RunTemplate(template,
                new
                {
                    items = new[]
                    {
                        new { text = "aaa" },
                        new { text = "bbb" },
                        new { text = "ccc" },
                    }
                },
                result3Items);

            RunTemplate(template,
                null,
                resultEmptyList);
        }

        [Fact]
        public void RendererTest_For_ShadowedVar()
        {
            var template =
@"<html>
<div>{{i}}</div>
<ul>
    {% for i in items %}
    <li>{{i.text}}</li>
    {% endfor %}
</ul>
<div>{{i}}</div>
</html>
";

            var result1Item =
@"<html>
<div>xyz</div>
<ul>
    <li>aaa</li>
</ul>
<div>xyz</div>
</html>
";

            RunTemplate(template,
                new
                {
                    i = "xyz",
                    items = new[] { new { text = "aaa" }, }
                },
                result1Item);
        }

        [Fact]
        public void RendererTest_For_Embedded()
        {
            var template =
@"<html>
<div>{{x}}</div>
{% for x in outer %}
    <div>start of round {{x}}</div>
    {% for x in inner %}
        <div>{{x}} is: 
        {% if x % 2 == 0 %}
            <span>even</span>
        {% else %}
            <span>odd</span>
        {% endif %}
    {% endfor %}
    <div>end of round {{x}}</div>
{% endfor %}
<div>{{x}}</div>
</html>
";

            RunTemplate(template, new
            {
                x = "hello",
                outer = new[] { 1, 2, 3 },
                inner = new[] { 1, 2, 3, 4, 5 }
            },
@"<html>
<div>hello</div>
    <div>start of round1</div>
        <div>1 is:
            <span>odd</span>
        <div>2 is:
            <span>even</span>
        <div>3 is:
            <span>odd</span>
        <div>4 is:
            <span>even</span>
        <div>5 is:
            <span>odd</span>
    <div>end of round1</div>
    <div>start of round2</div>
        <div>1 is:
            <span>odd</span>
        <div>2 is:
            <span>even</span>
        <div>3 is:
            <span>odd</span>
        <div>4 is:
            <span>even</span>
        <div>5 is:
            <span>odd</span>
    <div>end of round2</div>
    <div>start of round3</div>
        <div>1 is:
            <span>odd</span>
        <div>2 is:
            <span>even</span>
        <div>3 is:
            <span>odd</span>
        <div>4 is:
            <span>even</span>
        <div>5 is:
            <span>odd</span>
    <div>end of round3</div>
<div>hello</div>
</html>
");
        }
    }
}
