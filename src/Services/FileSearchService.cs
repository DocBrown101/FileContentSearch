namespace Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class FileSearchService
    {
        private readonly Action<int> fileCountChangedAction;
        private readonly SynchronizationContext? context;

        private DateTime lastUpdatDateTime;

        public FileSearchService(Action<int> fileCountChangedAction)
        {
            this.fileCountChangedAction = fileCountChangedAction;
            this.context = SynchronizationContext.Current;
        }

        public async Task<List<string>> LoadFilesForSearch(FileContentSearchOptions options, CancellationToken token)
        {
            this.NotifyFileCountChangedAction(0);
            this.lastUpdatDateTime = DateTime.Now;

            var files = await Task.Run(() =>
                                {
                                    return this.GetAllFilesBasedOnOptions(options, token);
                                },
                                token).ConfigureAwait(false);

            if (token.IsCancellationRequested) { return null; }

            this.NotifyFileCountChangedAction(files.Count);

            return files;
        }

        private List<string> GetAllFilesBasedOnOptions(FileContentSearchOptions options, CancellationToken token)
        {
            var allFiles = new List<string>();
            var pendingFolderPaths = new Queue<string>();
            pendingFolderPaths.Enqueue(options.SearchPath);

            while (pendingFolderPaths.Count > 0 && !token.IsCancellationRequested)
            {
                var folderPath = pendingFolderPaths.Dequeue();

                if (options.FileExtensions.Any())
                {
                    foreach (var fileExtension in options.FileExtensions)
                    {
                        allFiles.AddRange(Directory.EnumerateFiles(folderPath, $"*{fileExtension}"));
                    }
                }
                else
                {
                    allFiles.AddRange(Directory.EnumerateFiles(folderPath));
                }

                foreach (var folder in Directory.EnumerateDirectories(folderPath))
                {
                    if (token.IsCancellationRequested) { return new List<string>(); }

                    var currentFolderName = Path.GetFileName(folder);

                    if (currentFolderName != null && options.ExcludedSubdirectoryNames.Contains(currentFolderName))
                    {
                        continue;
                    }

                    if (CanRead(folder))
                    {
                        pendingFolderPaths.Enqueue(folder);
                    }
                }

                this.NotifyFileCountChangedActionCareful(allFiles.Count);
            }

            return allFiles;
        }

        private void NotifyFileCountChangedActionCareful(int fileCount)
        {
            var timeSpan = DateTime.Now - this.lastUpdatDateTime;

            if (timeSpan.TotalMilliseconds > 25)
            {
                this.lastUpdatDateTime = DateTime.Now;

                this.NotifyFileCountChangedAction(fileCount);
            }
        }

        private void NotifyFileCountChangedAction(int fileCount)
        {
            this.context?.Post(state => { this.fileCountChangedAction(fileCount); }, null);
        }

        private static bool CanRead(string folderPath)
        {
            try
            {
                Directory.EnumerateFiles(folderPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
