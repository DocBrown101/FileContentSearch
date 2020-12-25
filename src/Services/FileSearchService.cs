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
        private readonly SynchronizationContext context;

        private DateTime lastUpdatDateTime;

        public FileSearchService(Action<int> fileCountChangedAction)
        {
            this.fileCountChangedAction = fileCountChangedAction;
            this.context = SynchronizationContext.Current;
        }

        public async Task<List<string>> LoadFilesForSearch(FileContentSearchOptions fileContentSearchOptions, CancellationToken cancellationToken)
        {
            this.NotifyFileCountChangedAction(0);
            this.lastUpdatDateTime = DateTime.Now;

            var files = await Task.Run(
                            () =>
                                {
                                    var allFiles = this.GetAllFiles(fileContentSearchOptions.SearchPath, fileContentSearchOptions.ExcludedSubdirectoryNames, cancellationToken);

                                    return fileContentSearchOptions.FileExtensions.Any()
                                        ? allFiles.Where(file => fileContentSearchOptions.FileExtensions.Any(file.ToLower().EndsWith)).ToList()
                                        : allFiles;
                                },
                            cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            this.NotifyFileCountChangedAction(files.Count);

            return files;
        }

        private List<string> GetAllFiles(string rootFolderPath, ICollection<string> excludedSubdirectoryNames, CancellationToken cancellationToken)
        {
            var allFiles = new List<string>();
            var pendingFolderPaths = new Queue<string>();
            pendingFolderPaths.Enqueue(rootFolderPath);

            while (pendingFolderPaths.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var folderPath = pendingFolderPaths.Dequeue();
                allFiles.AddRange(Directory.EnumerateFiles(folderPath));

                foreach (var folder in Directory.EnumerateDirectories(folderPath))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    var currentFolderName = Path.GetFileName(folder);

                    if (currentFolderName != null && excludedSubdirectoryNames.Contains(currentFolderName))
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
            this.context.Post(state => { this.fileCountChangedAction(fileCount); }, null);
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
