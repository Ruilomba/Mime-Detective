namespace MimeDetective.Analyzers
{
    using System;
    public class DelimitedAnalyzer : IReadOnlyFileAnalyzer
    {
        const byte CarriageReturn = 0x0D;
        public byte Delimiter { get; }
        public DelimitedAnalyzer(char delimiter)
        {
            this.Delimiter = Convert.ToByte(delimiter);
        }

        public FileType Search(in ReadResult readResult)
        {
            var carriageReturnIndex = Array.FindIndex(readResult.Array, this.MatchPredicate);
            var firstLine = new byte[carriageReturnIndex];
            Array.Copy(readResult.Array, firstLine, carriageReturnIndex);
            var delimitersFirstLine = Array.FindAll(readResult.Array, MatchDelimiter);
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