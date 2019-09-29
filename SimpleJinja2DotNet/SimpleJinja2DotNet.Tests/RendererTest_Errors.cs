using Xunit;

namespace SimpleJinja2DotNet.Tests
{
    public class RendererTest_Errors
    {
        [Fact]
        public void RendererTest_Throws()
        {
            CheckError("{{-a}}", new { a = "a" }, 1, 4, RenderingError.UnsupportedOperation);
            CheckError("{{+a}}", new { a = "a" }, 1, 4, RenderingError.UnsupportedOperation);
            CheckError("{{1+a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{true+1}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{a+a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1-'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1.0-'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1*'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1.0*'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{a*a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1/'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1.0/'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{a/a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1//'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1.0//'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1.0//0.0}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{a//a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1%'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1.0%'a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1.0%0.0}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{a%a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{a>a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{a>=a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{a<a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{a<=a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{'a'==a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{'a'==1}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1=='a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1.0=='a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{true=='a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{'a'!=a}}", new { a = new object() }, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{'a'!=1}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1!='a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{1.0!='a'}}", null, 1, 3, RenderingError.UnsupportedOperation);
            CheckError("{{true!='a'}}", null, 1, 3, RenderingError.UnsupportedOperation);

            CheckError("{{'a'|unknown}}", null, 1, 7, RenderingError.UnsupportedFilter);

            CheckError("{{'a'|replace}}", null, 1, 7, RenderingError.FilterCallFailed);

            CheckError("{{1/0}}", null, 1, 5, RenderingError.DividedByZero);
            CheckError("{{1.0/0.0}}", null, 1, 7, RenderingError.DividedByZero);
            CheckError("{{1//0}}", null, 1, 6, RenderingError.DividedByZero);
            CheckError("{{1%0}}", null, 1, 5, RenderingError.DividedByZero);
        }

        private static void CheckError(
            string template, object globals,
            int row, int column, RenderingError error)
        {
            var t = Template.FromString(template);
            var ex = Assert.Throws<RenderingException>(() => t.Render(globals));
            Assert.Equal(row, ex.Row);
            Assert.Equal(column, ex.Column);
            Assert.Equal(error, ex.Error);
        }
    }
}
