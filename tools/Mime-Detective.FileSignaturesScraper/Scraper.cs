namespace Mime_Detective.FileSignaturesScraper
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AngleSharp;
    using AngleSharp.Dom;
    using MimeDetective;

    public class Scraper
    {
        private const string GridLinesSelector = "table#innerTable > tbody > tr";

        private const string PaginationSelector = "span#pageinate";

        private const string ExtensionSelector = "td:nth-child(1) > span#results > a";

        private const string MagicBytesSelector = "td:nth-child(2) > span#results > a";

        public Scraper()
        {
            var config = Configuration.Default.WithDefaultLoader();
            this.Context = BrowsingContext.New(config);
        }

        public IBrowsingContext Context { get; }

        public async Task<IEnumerable<FileType>> Scrape()
        {
            var address = "https://www.filesignatures.net/index.php?page=all&order=EXT";
            var page = 1;
            var lastPage = 0;
            do
            {
                address += "&currentpage=" + page;
                var document = await this.Context.OpenAsync(address);
                if (page == 1)
                {
                    this.ResolvePagination(document, out var currentPage, out lastPage);
                }

                var gridLines = document.QuerySelectorAll(GridLinesSelector);
                var fileTypes = this.ResolveFileTypes(gridLines);
                page++;
            } while (page <= lastPage);

            return fileTypes;
        }

        private IEnumerable<FileType> ResolveFileTypes(IHtmlCollection<IElement> gridLines)
        {
            foreach (var gridLine in gridLines)
            {
                yield return this.ResolveFileType(gridLine);
            }
        }

        private FileType ResolveFileType(IElement gridLine)
        {
            byte[] magicBytes;
            string extension;
            string mime;
            ushort size = 0;
            ushort offset = 0;
            var extensionElement = gridLine.QuerySelector(ExtensionSelector);
            if (extensionElement != null)
            {
                extension = extensionElement.TextContent;
            }

            var magicBytesElement = gridLine.QuerySelector(MagicBytesSelector);
            if (magicBytesElement != null)
            {

            }

            return new FileType(magicBytes, extension, null, offset);
        }

        private void ResolvePagination(
            IDocument document,
            out int currentPage,
            out int lastPage)
        {
            lastPage = currentPage = 0;
            var paginationElement = document.QuerySelector(PaginationSelector);
            if (paginationElement == null)
            {
                return;
            }

            var match = Regex.Match(paginationElement.TextContent, @"Page (?P<currentPage>\d*) of (?P<lastPage>\d*)");
            if (!match.Success)
            {
                return;
            }

            currentPage = Convert.ToInt32(match.Groups["currentPage"].Value);
            lastPage = Convert.ToInt32(match.Groups["lastPage"].Value);
        }
    }
}