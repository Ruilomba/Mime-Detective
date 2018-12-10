namespace MimeDetective.Analyzers
{
    using System;

    public class DelimitedAnalyzer
        : IReadOnlyFileAnalyzer
    {
        private const byte CarriageReturn = 0x0D;

        public byte Delimiter { get; }

        public ushort SampleLines { get; }

        public DelimitedAnalyzer(char delimiter)
            : this(delimiter, 2)
        { }

        public DelimitedAnalyzer(
            char delimiter,
            ushort sampleLines)
        {
            this.Delimiter = Convert.ToByte(delimiter);
            this.SampleLines = sampleLines;
        }

        public FileType Search(in ReadResult readResult)
        {
            var carriageReturnIndex = Array.FindIndex(readResult.Array, this.MatchPredicate);
            if (carriageReturnIndex == -1)
            {
                return null;
            }

            for (int i = 0; i < this.SampleLines; i++)
            {
                var line = new byte[carriageReturnIndex];
                Array.Copy(readResult.Array, line, carriageReturnIndex);
                var delimitersLine = Array.FindAll(line, MatchDelimiter);
                if (delimitersLine.Length == 0)
                {
                    return null;
                }
            }

            var firstLine = new byte[carriageReturnIndex];
            Array.Copy(readResult.Array, firstLine, carriageReturnIndex);
            var delimitersFirstLine = Array.FindAll(readResult.Array, MatchDelimiter);
            if (delimitersFirstLine.Length == 0)
            {
                return null;
            }

            var firstLineDelimitersCount = delimitersFirstLine.Length;
            var secondLine = new byte[carriageReturnIndex];
            Array.Copy(readResult.Array, carriageReturnIndex + 1, secondLine, 0, carriageReturnIndex);
            var delimitersSecondLine = Array.FindAll(readResult.Array, MatchDelimiter);
            var secondLineDelimitersCount = delimitersSecondLine.Length;
            if (secondLineDelimitersCount == firstLineDelimitersCount)
            {
                return MimeTypes.CSV;
            }

            return null;
        }

        private bool MatchPredicate(byte obj)
        {
            return obj == CarriageReturn;
        }

        private bool MatchDelimiter(byte obj)
        {
            return obj == Delimiter;
        }
    }
}