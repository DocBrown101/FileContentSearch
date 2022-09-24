namespace Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class FileContentSearchService
    {
        private readonly Action<FileContentSearchResult, int> fileContentSearchResultAction;
        private readonly ILocalizationService localizationService;
        private readonly SynchronizationContext synchronizationContext;

        public FileContentSearchService(Action<FileContentSearchResult, int> fileContentSearchResultAction, ILocalizationService localizationService)
        {
            this.fileContentSearchResultAction = fileContentSearchResultAction;
            this.localizationService = localizationService;
            this.synchronizationContext = SynchronizationContext.Current;
        }

        public async Task StartSearch(IReadOnlyList<string> files, FileContentSearchOptions searchOptions, CancellationToken cancellationToken)
        {
            var options = new ParallelOptions() { CancellationToken = cancellationToken };

            try
            {
                await Parallel.ForEachAsync(files, options, (searchFile, ct) =>
                {
                    this.SearchText(searchFile, files.Count, searchOptions, ct);

                    return ct.IsCancellationRequested ? ValueTask.FromCanceled(ct) : ValueTask.CompletedTask;
                }).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void SearchText(string searchFile, int fileCount, FileContentSearchOptions searchOptions, CancellationToken cancellationToken)
        {
            var fileInfo = new FileInfo(searchFile);

            if (!fileInfo.Exists)
            {
                this.NotifyResultAction(null, fileCount);
            }
            else
            {
                var searchTextFound = ExistsTextInFile(
                    fileInfo,
                    searchOptions.SearchTags,
                    searchOptions.CheckUpperLowerCase,
                    cancellationToken);

                if (searchTextFound == null)
                {
                    this.NotifyResultAction(new FileContentSearchResult(this.localizationService, fileInfo, searchOptions.SearchPath, true), fileCount);
                }
                else if (searchTextFound.Value)
                {
                    this.NotifyResultAction(new FileContentSearchResult(this.localizationService, fileInfo, searchOptions.SearchPath, false), fileCount);
                }
                else
                {
                    this.NotifyResultAction(null, fileCount);
                }
            }
        }

        private static bool? ExistsTextInFile(FileInfo fileInfo, ICollection<string> searchTags, bool upperLowerCase, CancellationToken cancellationToken)
        {
            var stringComparison = upperLowerCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            return fileInfo.Extension.Contains(".docx", StringComparison.OrdinalIgnoreCase)
                ? ExistsTextInDocxFile(fileInfo, searchTags, stringComparison)
                : ExistsTextInTextFile(fileInfo, searchTags, stringComparison, cancellationToken);
        }

        private static bool? ExistsTextInDocxFile(FileInfo fileInfo, ICollection<string> searchTags, StringComparison stringComparison)
        {
            try
            {
                var docxText = new DocxToTextService().ExtractText(fileInfo.FullName);

                foreach (var searchTag in searchTags)
                {
                    if (docxText.IndexOf(searchTag, stringComparison) <= -1)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return null;
            }
        }

        private static bool? ExistsTextInTextFile(FileInfo fileInfo, ICollection<string> searchTags, StringComparison stringComparison, CancellationToken cancellationToken)
        {
            try
            {
                var encoding = GetEncodingFromBOM(fileInfo.FullName);

                var foundSearchTags = new HashSet<string>();

                using (var streamReader = new StreamReader(fileInfo.FullName, encoding))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null && !cancellationToken.IsCancellationRequested)
                    {
                        foreach (var searchTag in searchTags)
                        {
                            if (line.IndexOf(searchTag, stringComparison) > -1)
                            {
                                foundSearchTags.Add(searchTag);
                            }
                        }

                        if (foundSearchTags.Count >= searchTags.Count)
                        {
                            break;
                        }
                    }
                }

                return foundSearchTags.Count == searchTags.Count;
            }
            catch
            {
                return null;
            }
        }

        private static Encoding GetEncodingFromBOM(string filename)
        {
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

#pragma warning disable SYSLIB0001 // Typ oder Element ist veraltet
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
#pragma warning restore SYSLIB0001 // Typ oder Element ist veraltet
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode;
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode;
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);

            // We actually have no idea what the encoding is if we reach this point
            var detectedEncoding = DetectFileEncoding(filename);

            return Encoding.GetEncoding(detectedEncoding);
        }

        private static string DetectFileEncoding(string filename)
        {
            var utf8EncodingVerifier = Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderExceptionFallback());

            using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(fileStream, utf8EncodingVerifier, true);
            string detectedEncoding;

            try
            {
                while (!streamReader.EndOfStream)
                {
                    _ = streamReader.ReadLine();
                }
                detectedEncoding = streamReader.CurrentEncoding.BodyName;
            }
            catch
            {
                // Assume it's local ANSI
                // TODO: Encoding.Default
                detectedEncoding = "ISO-8859-1";
            }

            return detectedEncoding;
        }

        private void NotifyResultAction(FileContentSearchResult searchTextResult, int fileCount)
        {
            this.synchronizationContext.Post(state => { this.fileContentSearchResultAction(searchTextResult, fileCount); }, null);
        }
    }
}
