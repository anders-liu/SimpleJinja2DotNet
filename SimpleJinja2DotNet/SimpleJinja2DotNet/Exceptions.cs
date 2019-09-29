using System;

namespace SimpleJinja2DotNet
{
    public enum ParsingError
    {
        IncompletedStatementBlock,
        IncompletedExpressionBlock,

        UnknownClause,

        MissingTestExpression,
        MissingLoopVariable,
        MissingInKeyword,
        MissingIterator,
        UnknownStatementType,
        MissingEndIf,
        MissingEndFor,

        MissingFilterCall,
        InvalidFilter,
        MissingParenthesis,
        MissingExpression,
        MissingEndOfSubscript,
        InvalidSymbol,
        InvalidNumber,
    }

    public class ParsingException : Exception
    {
        private static string FormatMessage(int charIndex, int row, int column, ParsingError error)
            => $"[{row},{column}](@{charIndex}): {error}";

        internal ParsingException(int charIndex, int row, int column, ParsingError error)
            : base(FormatMessage(charIndex, row, column, error))
        {
            CharIndex = charIndex;
            Row = row;
            Column = column;
            Error = error;
        }

        public int CharIndex { get; }

        public int Row { get; }

        public int Column { get; }

        public ParsingError Error { get; }
    }

    public enum RenderingError
    {
        UnsupportedOperation,
        UnsupportedFilter,
        FilterCallFailed,
        DividedByZero,
    }

    public class RenderingException : Exception
    {
        private static string FormatMessage(int charIndex, int row, int column, RenderingError error)
            => $"[{row},{column}](@{charIndex}): {error}";

        internal RenderingException(int charIndex, int row, int column,
            RenderingError error, Exception innerException = null)
            : base(FormatMessage(charIndex, row, column, error), innerException)
        {
            CharIndex = charIndex;
            Row = row;
            Column = column;
            Error = error;
        }

        public int CharIndex { get; }

        public int Row { get; }

        public int Column { get; }

        public RenderingError Error { get; }
    }
}
