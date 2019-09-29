using Xunit;

namespace SimpleJinja2DotNet.Tests
{
    internal static class RendererTestUtils
    {
        internal static void RunTemplate(
            string str, object globals, string expected)
        {
            var template = Template.FromString(str);
            var actual = template.Render(globals);
            Assert.Equal(expected, actual);
        }
    }
}
