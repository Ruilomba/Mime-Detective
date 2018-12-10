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
            if(Array.IndexOf(readResult.Array,CarriageReturn) == -1)
            {
                return null;
            }

            var delimiters = 0;
            var prevDelimiters = 0;
            int i = 0;
            foreach (var fileByte in readResult.Array)
            {
                if (i >= SampleLines)
                {
                    break;
                }

                if (fileByte == Delimiter)
                {
                    delimiters++;
                }

                if (EnfOfLine(fileByte))
                {
                    if (delimiters == 0)
                    {
                        return null;
                    }

                    if (prevDelimiters != 0)
                    {
                        if (prevDelimiters != delimiters)
                        {
                            return null;
                        }
                    }
                    i++;
                    prevDelimiters = delimiters;
                    delimiters = 0;
                }
            }
            return MimeTypes.CSV;
        }

        private static bool EnfOfLine(byte fileByte)
        {
            return fileByte == CarriageReturn;
        }
    }
}