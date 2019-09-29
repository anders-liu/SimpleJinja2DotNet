using System.Diagnostics;

namespace SimpleJinja2DotNet
{
    internal static class TextUtils
    {
        internal static (int row, int column)
            GetCharPosition(string s, int charIndex)
        {
            Debug.Assert(s != null);

            bool isNewLine(int currentIndex)
            {
                return currentIndex < s.Length &&
                    (s[currentIndex] == '\r' || s[currentIndex] == '\n');
            }

            var row = 1;
            var column = 1;
            var i = 0;
            var encounteredNewLine = isNewLine(i);
            if (i < s.Length - 1 && s[i] == '\r' && s[i + 1] == '\n')
                i++;

            while (i < s.Length && i < charIndex)
            {
                if (encounteredNewLine)
                {
                    row++;
                    column = 1;
                }
                else
                {
                    column++;
                }

                i++;
                encounteredNewLine = isNewLine(i);
                if (i < s.Length - 1 && s[i] == '\r' && s[i + 1] == '\n')
                    i++;
            }

            return (row, column);
        }
    }
}
